<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js"></script>
const otherUser = "@otherUser";
const currentUser = "@currentUser";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

connection.onreconnecting(() => console.warn("Reconnecting..."));
connection.onreconnected(() => console.warn("Reconnected"));
connection.onclose(err => console.error("Closed:", err));

connection.on("ReceiveMessage", function (user, message) {
    const li = document.createElement("li");
    li.textContent = `${user}:${message}`;

    document.getElementById("messagesList").appendChild(li);
});
connection.start().catch(function (err) {
    return console.error(err.toString());
});
function sendMessage() {
    const user = document.getElementById("userInput").value;
    const message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
}



const messagesList = document.getElementById("messagesList");
const messageInput = document.getElementById("messageInput");
const sendBtn = document.getElementById("sendBtn");
const presence = document.getElementById("presence");

function escapeHtml(s) {
    const div = document.createElement("div");
    div.innerText = s ?? "";
    return div.innerHTML;
}

function appendMessage(user, message, ts) {
    const li = document.createElement("li");
    li.classList.add("message", user === currentUser ? "from-me" : "from-others");
    const time = ts ? new Date(ts).toLocaleTimeString() : "";
    li.innerHTML = `
    <div class="bubble">
      <div class="text">${escapeHtml(message)}</div>
      <div class="meta">${escapeHtml(user)} · ${time}</div>
    </div>`;
    messagesList.appendChild(li);
    messagesList.scrollTop = messagesList.scrollHeight;
}

connection.on("ReceiveMessage", (user, message, timestamp) => {
    appendMessage(user, message, timestamp);
});

connection.on("PresenceChanged", (user, state) => {
    if (user === otherUser) {
        presence.textContent = state;
        presence.className = "presence " + state;
    }
});

connection.start()
    .then(() => {
        console.log("Connected, JoinDialoug:", otherUser);
        return connection.invoke("JoinDialoug", otherUser);
    })
    .then(() => console.log("JoinDialoug done"))
    .catch(err => console.error("Start/Join error:", err));

async function sendMessage() {
    const text = messageInput.value.trim();
    if (!text) return;
    try {
        await connection.invoke("SendMessage", otherUser, text);
        appendMessage(currentUser, text, new Date().toISOString());
        messageInput.value = "";
        messageInput.focus();
    } catch (err) {
        console.error("SendMessage error:", err);
    }
}
sendBtn.addEventListener("click", sendMessage);
messageInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

window.addEventListener("beforeunload", () => {
    connection.invoke("LeaveDialog", otherUser).catch(() => { });
});