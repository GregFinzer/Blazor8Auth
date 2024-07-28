# Custom Authentication with Blazor 8 and Server Interactive Render Mode

TLDR; Clone the GitHub repository below.  Review the Login.razor.cs HandleLogin method how it calls the AuthDataService for the custom authentication.  Then look at the code for the AuthService.cs Login method.  Finally, look at the MainLayout.razor OnAfterRenderAsync method how it refreshes the page if the session is lost.

https://github.com/GregFinzer/Blazor8Auth

## Blazor 8 Built In Authentication
Blazor 8 provides authentication out of the box with its Identity Template and Blazor Server Interactive.  It is a lot of boilerplate code that most likely will not be used by your enterprise.

The steps to create are:
1.	Create a new application in Visual Studio
2.	Choose Blazor Web App
3.	Choose Individual Accounts for the Authorization
4.	Choose Blazor Server
5.	Choose Component Interactive
This works fine if you want to have Blazor automatically create a bunch of tables with Entity Framework and use all the baked in templates and endpoints.  

Patrick God does a great job explaining this in his YouTube video.

[![.NET 8 Blazor Authentication](https://img.youtube.com/vi/tNzSuwV62Lw/0.jpg)](https://www.youtube.com/watch?v=tNzSuwV62Lw)

## The Misery of the Internet
However, what if you don’t want to be married to Entity Framework or you have an external custom authentication provider that maintains the session in a cookie?  

### A lot of other people are stuck
https://www.reddit.com/r/Blazor/comments/1abr7u7/how_do_you_persist_login_for_a_user_in_blazor_net/

https://www.reddit.com/r/Blazor/comments/1cop6qf/is_there_an_easy_way_to_implement_login_and/

https://github.com/dotnet/AspNetCore.Docs/issues/16813

https://stackoverflow.com/questions/78111367/blazor-server-net-8-cookie-authentication-without-identity

https://stackoverflow.com/questions/78666405/authentication-in-blazor-server-interactive

https://stackoverflow.com/questions/77698959/blazor-net-8-own-authentication

https://stackoverflow.com/questions/78703378/blazor-web-app-net-8-cookie-authentication-without-identity-interactiveserverr


I created my own Stack Overflow question, Microsoft Forum Question, and Blazor Forum Question.  No one, not even Microsoft could figure it out.

#### Stack Overflow
https://stackoverflow.com/questions/78791143/how-to-maintain-authentication-state-with-blazor-8-server-interactive

#### Microsoft Forums
https://learn.microsoft.com/en-us/answers/questions/1841065/how-to-maintain-authentication-state-with-blazor-8

#### Blazor Forums
https://blazorforums.net/topic/221/how-to-maintain-authentication-state-with-blazor-8-server-interactive

## Crashing and Burning
I have spent over a week pulling my hair out.  There are two main problems:
1.  HttpContext will be null for Server Interactive Render Mode.  You can't use SignInAsync.  In fact Microsoft recommends that you don't use the HttpContext.  As soon as you do this in your App.razor you are screwed.

    `<Routes @rendermode="RenderMode.InteractiveServer" />`

2.  Unlike Blazor 7, the Blazor 8 AuthenticationState provider fires during the OnInitializedAsync event instead of the OnAfterRenderAsync event for the GetAuthenticationState method.  This means you cannot get cookies from the Browser.

There are many videos on YouTube that will lead you down the wrong path.

### Amazingly Bad
This guy creates a static variable for storing the JWT so his application really only supports a single user. 
[![Custom JWT Authentication](https://img.youtube.com/vi/EFvR2EXDam8/0.jpg)](https://www.youtube.com/watch?v=EFvR2EXDam8)

### Server Side Rendering Failure
It is not possible to use HttpContext.SignInAsync with Blazor Server Interactive (it only works for SSR, Server Side Rendering).  HttpContext will be null even when using the CascadingState.  Wrong direction.  

[![Blazor Web App Authentication and Authorization](https://img.youtube.com/vi/GKvEuA80FAE/0.jpg)](https://www.youtube.com/watch?v=GKvEuA80FAE)

### Waste an Hour of Your Life
This is a discussion about Authentication where nothing is accomplished

[![Video Title](https://img.youtube.com/vi/uVCzdDrXh0Y/0.jpg)](https://www.youtube.com/watch?v=uVCzdDrXh0Y)

### So what now?
All along the way, people told me just to read the documentation, which I did, over and over.
https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-8.0

https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/?view=aspnetcore-8.0&tabs=visual-studio

## The Solution
I came up with what I am hoping is a temporary solution until someone responds with something better.  Blazor keeps track of the session on the back end and this solution also maintains a duplicate JWT in a Browser Session Cookie.  When AuthService.cs logs in, it saves it to both the back end session and the Browser session.  When it logs out, it clears both.  It also has the ability to get the browser session if the back end session is lost.

Review the Login.razor.cs HandleLogin method how it calls the AuthDataService for the custom authentication. Then look at the code for the AuthService.cs Login method. Finally, look at the MainLayout.razor OnAfterRenderAsync method how it refreshes the page if the session is lost.

1.	**AuthService.cs** – It stores the session as a JWT as a Session Cookie using Blazored.SessionStorage.  It handles logging in, logging out, and restoring the session.
2.	**CustomAuthStateProvider.cs** – It will notify the Blazor backend session when you are logged in.
3.	**AuthDataService.cs** – This represents a custom external provider.  It could be a database or some other external auth provider.
4.	**MainLayout.razor** – It will force a refresh of the page when the Blazor back end session has been lost. 
5.	**Login.razor.cs** – It will automatically login and redirect back to a page if the back end session is lost and there is still a session cookie.
6.  **CheckAuthorization.razor** - If the user is already logged in then it will display a message saying "Sorry, you are not authorized to view this page." otherwise it will redirect the user to the login page.

### Use Cases
#### Normal Login
When the user goes to the login page and logs in, the back end session is updated for the CurrentUser and the session is set to the Browser Session JWT Cookie.

#### Normal Logout
When the user goes to the logout page, it clears the back end session and then removes the Browser Session JWT Cookie.

#### Hard Reset
When a hard reset is performed on any page, Main.Layout.razor will attempt to get the session from the browser and then refresh the page.  Log in and go to the /auth page.  Then do a control-F5 on your browser.

#### Deep Link Authentication and Not Logged In
When a user deep links to a page that requires authentication and they are not authenticated, it will redirect them to login.  To test this, go directly to the /mustBeLoggedIn page and it will redirect to the login.

#### Deep Link Authentication and Back End Session Lost
When a user deep links to a page that requires authentication and they have lost their back end session but they still have a browser session, they will be redirected to login, the session restored, and then redirected back to the page they were on.  To test this scenario, login then go to the /mustBeLoggedIn and do a Control-F5 on your browser.

#### Deep Link Authentication, Session Lost, No Access
When a user deep links to a page and they have lost their back end session, and that page requires a certain role but they do not have it, it will attempt to restore their session and then display a message.  To test this, login as super@acme.com and then go to the /mustBeAdmin and do a Control-F5 on your browser.

### Updates to This Repository
I am hoping that someone has an idea how to simplify this repository based on implementing classes built into Blazor 8 identity.  So far, this is the only thing that works for Custom Authorization and Authentication.