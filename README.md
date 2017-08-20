# CloudScribe with VueJs and Javascript Services

I have recently started playing with CloudScribe and it seems very compelling to those of us who write apps for the business community on a day to day basis. This example still uses netcoreapp1.1, not 2.0.

This is an example of integrating a [VueJS](https://vuejs.org/)/[Javascript Services](https://github.com/aspnet/JavaScriptServices) project with the [CloudScribe](https://www.cloudscribe.com/) multi-tenant framework for AspNetCore. It's not really documented, or meant to be an answer to anying, it's just an idea. If anyone has feedback or ideas that would be great. If people find this useful maybe we could create a template generator. 


There is a VueJs plugin for routing ajax requests to the correct tenant url (i.e. including the site specific path). Note I have only played with the site folder per tenant option in Cloud Scribe. This allows the existing security pipeline to 'just work' for any ajax requests per tenant. There is no limit to the number of distince VueJs SPAs you can host per tenant.


I have just started playing with Javascript Services and I really like having the ability to edit both the client code and server code while having things like hot reloading for both, as well as being able to run node javascript on the server if I need it for processing or whatever.

To Run:

:>cd /<< your workspace root >>/src/OPServer

:>dotnet restore

:>npm install

:>npm start

:>dotnet watch run

The Vue app is pretty much the same app that comes with the built in Javascript Services VueJs template.

Hopefully it helps someone. If you have any ideas or problems let me know.
