''
' This script displays the CPU usage and Memory usage of
' all running COM+ applications on a specific machine 
' periodically.  It is defaulted to refresh every 5 secs
' and monitor the local machine.  Use Control-C to quit
' the script.
'
' Usage:
' cscript mon_complus.vbs >> f:\my_log_file.txt 
'
' As of 2007 03 22
'
COMPUTER_NAME = "."
REFRESH_IN_MSEC = 5000


''
' Return an array of running COM+ Application name and process Id.
' @return   An array of 32 rows and 2 columns (name, process Id).
'
Function GetRunningComPlusApps()
    Dim cat 'as COMAdminCatalog
    Set cat = CreateObject("COMAdmin.COMAdminCatalog")

    Dim apps 'as COMAdinCatalogCollection
    Set apps = cat.GetCollection("Applications")
    apps.Populate
     
    Dim instances 'as COMAdminCatalogCollection
    Set instances = cat.GetCollection("ApplicationInstances")
    instances.Populate

    Dim instance 'as COMAdminCatalogObject
    Dim appID 'as String
    Dim app 'as COMAdminCatalogObjecy
    Dim appName 'as String

    Dim retVal(31, 1) 'as 2 dimension return array
    Dim j 'as current row counter
    j = 0
    For Each instance In instances
        appID = instance.Value("Application")
        For Each app In apps
            If appID = app.Value("ID") Then
                appName = app.Name
                Exit For
            End If
        Next
        retVal(j, 0) = instance.Value("ProcessID")
        retVal(j, 1) = appName 
        j = j + 1
    Next

    GetRunningComPlusApps = retVal

    Set appID = Nothing
    Set appName = Nothing
    Set app = Nothing
    Set apps = Nothing
    Set instance = Nothing
    Set instances = Nothing
    Set cat = Nothing
End Function


''
' The main loop.  Every so many seconds we query COM+ for
' the list of running COM+ apps, then we query WMI for the
' list of running processes.  We match up the COM+ apps in
' both lists to find their performance metrics.
'
On Error Resume Next 

    Dim objWMIService
    Dim colProcess
    Dim arrComPlusApps

    WScript.Echo ("Time" & "," & _ 
                  "ComputerName" & "," & _
                  "ProcessName(ID)" & "," & _
                  "%ProcessorTime" & "," & _
                  "%UserTime" & "," & _
                  "ThreadCount" & "," & _
                  "PrivateBytes" & "," & _
                  "WorkingSet") 
    Do While 1=1  
        arrComPlusApps = GetRunningComPlusApps() 
        Set objWMIService = GetObject("winmgmts:\\" & COMPUTER_NAME & "\root\cimv2")
        Set colProcess = objWMIService.ExecQuery("Select * from Win32_PerfFormattedData_PerfProc_Process",,48)
               
        For Each objItem in colProcess	
            if objItem.Name = "dllhost" then 
                Dim i
                For i = 0 to UBound(arrComPlusApps, 1)
                    If arrComPlusApps(i, 0) = objItem.IDProcess Then
                        WScript.Echo (now() & _ 
                            "," & COMPUTER_NAME & _
                            "," & arrComPlusApps(i, 1) & "(" & arrComPlusApps(i, 0) & ")" & _
                            "," & objItem.PercentProcessorTime & _
                            "," & objItem.PercentUserTime & _
                            "," & objItem.ThreadCount & _
                            "," & objItem.PrivateBytes & _
                            "," & objItem.WorkingSet)
                        Exit For
                    End If
                Next 
            end if
        Next

        WScript.Sleep(REFRESH_IN_MSEC)
    Loop

