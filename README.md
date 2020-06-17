# Concord-Chat
Socket Program using client and server with a local SQL Database

ConcordChat Formerly SGS Chat, is a Socket program that works via TCP protocols. To allow users to communicate with one another through simple message sending. On launch the user is greeted with a small login page to create an account and login to said account, after verifying their account they can login to a server of their choice via IP.

1.1	Connection String
ensure connection string on Concord Client is correct.
view the following image to see an example.
https://imgur.com/a/ep7IwgW
 
1.2	How to setup
Upon opening the Zip containing there will be 2 folders, CServer and CClient. Run the solution or the exe in the /debug folder of CServer first. The default Port of CServer is 1000, and the default IP is 127.0.0.1 This is for easy testing purposes to ensure no one enters the wrong information. 
Next launch CClient and simply login/create an account and enter in the IP (127.0.0.1, will be in the text box by default) Hit connect and you will be greeted with the landing page where you can now message others.
