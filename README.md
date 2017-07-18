Splicr
==============================

A proxy server to splice together micro-frontends. 

This is an experiment to create an open-source version of the production micro-frontend framework that powers www.vistaprint.com.
## About

### Responsibilities
* Route to different backends based on pattern matching
  * Match and transform routes based on either regexes or arbitrary code
* Splice content from backends together with a consistent page layout (i.e. header and footer)
  * Backend pages can select/configure versions of the shared layout (i.e. standard vs minimal)
* Initialize sessions
  * Set a session cookie for requests without them, invoke a backend session initialization service, either synchronously or asynchronously

### Types of backends
* Standard Backend: Requests are routed to a backend via pattern matching
* Layout backend: Layouts can be defined as a URL, pointing to a backend that provides the layout template HTML
* Session initialization backend: You can specify a service endpoint to notify when a session is initialized

### Assumptions
* Shared layouts should be static and cacheable (not vary by user)
  * Per-user variation should be implemented via client-side ajax
  * Makes it possible to minimize the performance overhead, while preserving the ability for backends to configure the layout
* We should be able to find a good way to specify custom backend routing logic to do things that aren't possible with regex. We'll need a plug-in or scripting mechanism.
* Only the minimum configuration necessary should be included into the splicr server itself. As much power as possible should be delegated to backends.
  * i.e. You could specify page layouts with only a URL to a service, and all the detailed functionality about splicing/configuring them could be implemented via an API 
  * We can use custom HTTP headers as the API to communicate between splicr and the backends

### Questions
* Would it be better to try to do this with Varnish or Ngnix? Are they extensible enough to fulfill the responsibilities (i.e. splicing layouts, arbitrary code for routing)?  
* How about https://www.mosaic9.org/ - this seems like a more extensive ecosystem, more flexible than simple splicing content into layouts. However, this flexibility might come at a performance/complexity cost, compared to the much simpler use-case that splicr is going for.

## Notes
* We might use https://github.com/madskristensen/ReverseProxyCDN for technical inspiration on building a proxy in .NET
* This looks like it handles the richness of HTTP: https://github.com/aspnet/Proxy 

## Setup

1. Install dotnet core: https://www.microsoft.com/net/download/core
2. Install node.js (to create a simple static server for mocking out backends): https://nodejs.org/en/
3. Start the content server

```
npm install -g http-server
cd content
http-server -p 5001
```

4. Start splicr:
```
dotnet restore
dotnet run
```

Hit a spliced URL, simulating a backend: http://localhost:5000/content1/content.html
