const otherUser = "@otherUser";
const currentUser = "@currentUser";
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

connection.onreconnecting(() => console.warn("Reconnecting..."));
connection.onreconnected(() => console.warn("Reconnected"));
connection.onreclose(() => console.error("Closed:", err));

const messageList = document.getElementById("messageList");
const messageInput = document.getElementById("messagesInput");
const sendBtn = document.getElementById("sendBtn");
const presence = document.getElementById("presence");

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
    document.getElementById("messageList").appendChild(li);
    const messageList = document.getElementById("messageList");
    messageList.scrollTop = messageList.ScrollHeight;
});

connection.start()
    .then(() => {
        console.log("Connected, JoinDialog:", otherUser);
        return connection.invoke("JoinDialouge", otherUser);
    })
    .then(() => console.log("JoinDialoge done"))
    .catch(err => console.error("Start/Join error:", err));

connection.on("ReceiveMessage", (user, message, timestamp) => {
    appendMessage(user, message, timestamp);
});

connection.on("PresenceChanged", (user, state) => {
    if (user === otherUser) {
        presence.textContent = state;
        presence.className = "presence" + state;
    }
});

sendBtn.addEventListener("click", sendMessage);
messageInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

async function sendMessage() {
    const text = messageInput.value.trim();
    if (!text) return;
    try {
        await connection.invoke("SendMessage", otherUser, text);
        appendMessage(currentUser, text, new Date().toISOString());
        messageInput.value = "";
        messageInput.focus();

    } catch {
        console.error("SendMessage error:", err);
    }
}
function appendMessage(user, message, ts) {
    const li = document.createElement("li");
    li.classList.add("message", user === currentUser ? "from-me" : "from-others");
    const time = ts ? new Date(ts).toLocaleTimeString() : "";
    li.innerHTML = `
    <div class="bubble">
        <div class="text">${escapeHtml(message)}</div>
        <div class="meta">${escapeHtml(user)}</div>
    </div>
    `;
    messageList.appendChild(li);
    messageList.scrollTop = messageList.ScrollHeight;
}

function escapeHtml(s) {
    const div = document.createElement("div");
    div.innerText = s ?? "";
    return div.innerHTML;
}