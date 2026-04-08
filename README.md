This repository has a web application that reproduces an issue in Azure App Services where a TCP connection is not 
cleanly shut down through Azure Hybrid Connect, leaving the connection in a `CLOSE_WAIT` state in the app service. I 
have found this issue occurs on Linux App Service plans when the server is a Windows Server. I have not tested on any 
other server systems.

## Steps to reproduce
- On a Windows server with the .NET 10 runtime, run the `SocketServer` project. (Starts listening on any IP on port 5000)
- Install the Hybrid Connection Manager on the Windows Server.
- Create an Azure Relay resource. Within that resource, create a new Hybrid Connection, taking note of the host name as well as setting the port to 5000.
- Create Azure App Service on a Linux App Service plan and connect the hybrid connection created in the previous step. The 
- In the `MinimumRepro/Pages/Index.cshtml.cs:25`, update the host name (replacing `******`) with the host name created for the hybrid connection.
- Publish the MinimumRepro application to the app service. 
- When the application is ready, send a few requests. Alternatively, set the application to always on and leave the app running for several minutes. If the connection to the Windows Server is successful, the index page will read the UTC timestamp from the Windows Server plus `<|ACK|>`.
- Open an SSH console into the application and run `netstat`. Observe several connections in the `CLOSE_WAIT` state for the port 5000.

## tcpdump from good exchange
This is run from a macOS system that is able to reach the server via a GlobalProtect gateway. Running `netstat` a few 
moments after the sending the request to the web app produces no entries for the 5000 port. The IP address of client and
server has been redacted.

```
15:15:29.989287 IP [client].53049 > [server].5000: Flags [SEW], seq 2397157413, win 65535, options [mss 1360,nop,wscale 6,nop,nop,TS val 849356722 ecr 0,sackOK,eol], length 0
15:15:30.026539 IP [server].5000 > [client].53049: Flags [S.E], seq 1560238901, ack 2397157414, win 65535, options [mss 1460,nop,wscale 8,nop,nop,sackOK], length 0
15:15:30.026648 IP [client].53049 > [server].5000: Flags [.], ack 1, win 4096, length 0
15:15:30.026766 IP [client].53049 > [server].5000: Flags [P.], seq 1:20, ack 1, win 4096, length 19
15:15:30.066248 IP [server].5000 > [client].53049: Flags [P.], seq 1:20, ack 20, win 1025, length 19
15:15:30.066274 IP [client].53049 > [server].5000: Flags [.], ack 20, win 4096, length 0
15:15:30.066326 IP [server].5000 > [client].53049: Flags [FP.], seq 20:27, ack 20, win 1025, length 7
15:15:30.066334 IP [client].53049 > [server].5000: Flags [.], ack 28, win 4096, length 0
15:15:30.067405 IP [client].53049 > [server].5000: Flags [F.], seq 20, ack 28, win 4096, length 0
15:15:30.103468 IP [server].5000 > [client].53049: Flags [.], ack 21, win 1025, length 0
```

## tcpdump from Azure App Services
What stands out in this tcpdump is the lack of the `F` flag in the `seq 20:27` line, fourth from the end. 

```
20:16:52.511569 lo    In  IP 127.0.0.1.43464 > 127.0.0.14.5000: Flags [S], seq 3489187527, win 65495, options [mss 65495,sackOK,TS val 3892565052 ecr 0,nop,wscale 7], length 0
20:16:52.511580 lo    In  IP 127.0.0.14.5000 > 127.0.0.1.43464: Flags [S.], seq 3508361967, ack 3489187528, win 65483, options [mss 65495,sackOK,TS val 1439098701 ecr 3892565052,nop,wscale 7], length 0
20:16:52.511591 lo    In  IP 127.0.0.1.43464 > 127.0.0.14.5000: Flags [.], ack 1, win 512, options [nop,nop,TS val 3892565052 ecr 1439098701], length 0
20:16:52.511950 lo    In  IP 127.0.0.1.43464 > 127.0.0.14.5000: Flags [P.], seq 1:20, ack 1, win 512, options [nop,nop,TS val 3892565053 ecr 1439098701], length 19
20:16:52.511959 lo    In  IP 127.0.0.14.5000 > 127.0.0.1.43464: Flags [.], ack 20, win 512, options [nop,nop,TS val 1439098702 ecr 3892565053], length 0
20:16:53.609867 lo    In  IP 127.0.0.14.5000 > 127.0.0.1.43464: Flags [P.], seq 1:20, ack 20, win 512, options [nop,nop,TS val 1439099800 ecr 3892565053], length 19
20:16:53.609891 lo    In  IP 127.0.0.1.43464 > 127.0.0.14.5000: Flags [.], ack 20, win 512, options [nop,nop,TS val 3892566151 ecr 1439099800], length 0
20:16:53.609918 lo    In  IP 127.0.0.14.5000 > 127.0.0.1.43464: Flags [P.], seq 20:27, ack 20, win 512, options [nop,nop,TS val 1439099800 ecr 3892566151], length 7
20:16:53.609924 lo    In  IP 127.0.0.1.43464 > 127.0.0.14.5000: Flags [.], ack 27, win 512, options [nop,nop,TS val 3892566151 ecr 1439099800], length 0
20:16:53.610322 lo    In  IP 127.0.0.1.43464 > 127.0.0.14.5000: Flags [F.], seq 20, ack 27, win 512, options [nop,nop,TS val 3892566151 ecr 1439099800], length 0
20:16:53.653900 lo    In  IP 127.0.0.14.5000 > 127.0.0.1.43464: Flags [.], ack 21, win 512, options [nop,nop,TS val 1439099844 ecr 3892566151], length 0
```