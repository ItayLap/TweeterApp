async function loadFriends() {
    return await (await fetch("/friends")).json();
}
async function loadRequests() {
    return await (await fetch("/friends/requests")).json();
}

async function sendFriendRequest(toUser) {
    return await fetch("/friends/request", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(toUser)
    });
}