const box = document.getElementById("chat-box");
const btnToggle = document.getElementById("chat-toggle");
const btnClose = document.getElementById("chat-close");
const messages = document.getElementById("chat-messages");
const input = document.getElementById("userMessage");
const btnSend = document.getElementById("btnSend");

btnToggle?.addEventListener("click", () => {
    box.classList.toggle("hidden");
    if (!box.classList.contains("hidden")) input.focus();
});

btnClose?.addEventListener("click", () => box.classList.add("hidden"));

btnSend?.addEventListener("click", sendMessage);
input?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") sendMessage();
});

function addBubble(sender, text, isHtml = false) {
    const row = document.createElement("div");
    row.className = `msg ${sender}`;

    const bubble = document.createElement("div");
    bubble.className = "bubble";
    if (isHtml) bubble.innerHTML = text;
    else bubble.textContent = text;

    row.appendChild(bubble);
    messages.appendChild(row);
    messages.scrollTop = messages.scrollHeight;
    return row;
}

function addTyping() {
    return addBubble("bot", `<span class="typing"><span></span><span></span><span></span></span>`, true);
}

async function sendMessage() {
    const text = (input.value || "").trim();
    if (!text) return;

    addBubble("user", text);
    input.value = "";
    input.focus();

    const typingRow = addTyping();

    try {
        const res = await fetch("/Chat/Ask", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ message: text })
        });

        const data = await res.json();
        typingRow.remove();
        addBubble("bot", data.reply ?? "⚠️ Không có phản hồi.");
    } catch (err) {
        typingRow.remove();
        addBubble("bot", "⚠️ Lỗi kết nối, vui lòng thử lại.");
    }
}
