# DataverseMerger

Change the 3 values in the appsettings section of the app.config file

The CSV file should be in the format below, where Master is the primary record ID that will be kept and Child is the subordinate record:

Master,Child  
c5144041-916c-f011-b4cc-7c1e5265f94f,285e8947-916c-f011-b4cc-7c1e5265f94f  
3cb1df26-c9aa-4209-b659-bfcbfb89f37d,b712354d-d878-4cd2-9d3c-52d43383d4be  
ff2be9b5-75c2-425a-9930-f0a5e88fbf14,d060f151-a9f8-45ae-8184-488a2a68310a  

Fields on the Master record are not modified even if different on the Child. All linked entities from the child are moved to the master (activities, cases, invoices etc).

Run DataverseMerger.exe from a command prompt after saving the app.conmfig

You may be prompted to log into Dataverse. It will use your cached login if you are already authenticated.

Requires .net Framework 4.6.2 or later.
