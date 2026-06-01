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
    <li>If you are using your local machine for 2 client instances, you are not required to enter the IP, since it uses the local (127.0.0.1) by default.</li>
    <li>If you want to play from 2 different devices with different WANs, you are required to use a VPN to be able to join the same game.</li>
    <li>When 2 players join the same room, the game automatically gets started.</li>
    <li>If a player disconnects midplay, the other client is sent back to the lobby.</li>
</ol>
