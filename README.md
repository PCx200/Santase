<h1>Santase</h1>

<p>
A server-client multiplayer game with a dedicated C# Server and Unity Client that connects to the server and provides the full game UI, animations, and interactions. The server fully controls the game state. The client only sends requests and receives updates.
</p>

<h2>Project Structure</h2>

<h3>1. TCP Server</h3>

<p>
The server is a standalone application. It contains the logic for room creation and joining, as well as assigning players to rooms and broadcasting game events to both players. The server uses OSC protocol messages for communication.
</p>

<p>
Server.cs accepts clients, manages the lobby, and routes packets. Room.cs holds the two players and contains a Game Model instance. GameModel.cs contains all the gameplay logic and rules. To make the server autoritative I have made each room hold and control the Game Model instance.
</p>

<h3>2. Unity Client</h3>

<p>
The client is responsible for listening to the server and responding by updating UI, animations, and SFXs, as well as containing the Game Controller - the central hub that receives server events. The client never simulates game logic - it only displays what the server says.
</p>

<h2>How to run the game:</h2>

<ol>
    <li>Download the zip file from my repository and extract it.</li>
    <li>Run the server .exe file from the /SantaseServer folder (keep the console open). The server listens on port 50001 by default.</li>
    <li>Start the client from the .exe file in the /Build folder.</li>
    <li>Create a room by entering a name and password (name should be unique - if there is a room with an already existing name, the client cannot create a room with that name).</li>
</ol>

<img width="1073" height="1518" alt="Lobby_Phase" src="https://github.com/user-attachments/assets/438a20ab-223d-4149-9033-871fa6aefb48" />

<img width="1721" height="910" alt="Room_Creation_and_Joining" src="https://github.com/user-attachments/assets/a487f0fb-81b8-4ce5-bfe4-6444fdfbdcb0" />

<ol start="5">
    <li>If you are using your local machine for 2 client instances, you are not required to enter the IP, since it uses the local (127.0.0.1) by default.</li>
    <li>If you want to play from 2 different devices with different WANs, you are required to use a VPN to be able to join the same game.</li>
    <li>When 2 players join the same room, the game automatically gets started.</li>
</ol>

<img width="2095" height="1528" alt="Game_Phase" src="https://github.com/user-attachments/assets/ff01c341-beeb-4fdd-9cda-d6d2a8a2f119" />

<ol start="8">
    <li>If a player disconnects midplay, the other client is sent back to the lobby.</li>
</ol>

<img width="1721" height="909" alt="Disconnection" src="https://github.com/user-attachments/assets/272991a9-681e-4cf3-b0b4-3bb2b188e34b" />
