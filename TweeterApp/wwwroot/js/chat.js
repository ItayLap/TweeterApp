const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();
const currentUsername = "@User.Identity?.Name";
connection.on("ReciveMessage", function (user, message){
    const li = document.createElement("li");
    li.classList.add("message");
    if (user === currentUsername) {
        li.classList.add("from-me")
    } else {
        li.classList.add("from-others")
    }
    li.textContent = `${user}: ${message}`;
    document.getElementById("messagesList").appendChild(li);
    const messageList = document.getElementById("messageList");
    messageList.scrollTop = messagesList.ScrollHeight;
});

connection.start().catch(err => console.error(err.toString()));

function sendMessage() {
    const user = document.getElementById("UserInput").value.trim();
    const message = document.getElementById("MessageInput").value.trim();
    if (!user || !message) return;
    connection.invoke("SendPrivateMessage", user, message).catch(err => console.error(err.toString()));
    document.getElementById("messageInput").value = "";
}