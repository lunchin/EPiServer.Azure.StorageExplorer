# EPiServer.Azure.StorageExplorer
Installation
------------

1.  Configure Visual Studio to add this package source: http://nuget.optimizely.com/feed/packages.svc/. This allows missing packages to be downloaded, when the solution is built.
2.  Open solution and build to download nuget package dependencies.
3.  This uses Local DB so no need to run database setup  
4.  Make sure to set a valid EPiServerAzureBlobs in the connectionstrings section of the web.config
5.  Start the site (Debug-Start from Visual studio) and browse to http://localhost:64319/ to finish installation. 

Logging in
-------------
To log in to the site, use u: admin  p: Episerver123!.

