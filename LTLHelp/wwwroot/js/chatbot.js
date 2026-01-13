document.addEventListener("DOMContentLoaded", function () {
    const chatBox = document.getElementById("chat-box");
    const chatMessages = document.getElementById("chat-messages");
    const userInput = document.getElementById("userMessage");
    const btnSend = document.getElementById("btnSend");
    const chatToggle = document.getElementById("chat-toggle");
    const chatClose = document.getElementById("chat-close");
    const clearBtn = document.getElementById("clear-chat");

    // 1. Tải lịch sử từ trình duyệt
    const history = JSON.parse(localStorage.getItem("chatHistory") || "[]");
    history.forEach(m => renderMsg(m.text, m.sender));

    function renderMsg(text, sender) {
        const div = document.createElement("div");
        div.className = `msg ${sender}`; // 'user' hoặc 'bot'
        div.innerText = text;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // 2. Hàm gửi tin nhắn
    async function send() {
        const msg = userInput.value.trim();
        if (!msg) return;

        // Hiển thị tin nhắn người dùng ngay lập tức
        renderMsg(msg, "user");
        userInput.value = ""; // Xóa nội dung trong ô nhập
        userInput.focus();    // Giữ con trỏ chuột ở ô nhập

        // Lưu vào localStorage
        const currentHistory = JSON.parse(localStorage.getItem("chatHistory") || "[]");
        currentHistory.push({ text: msg, sender: "user" });
        localStorage.setItem("chatHistory", JSON.stringify(currentHistory));

        try {
            const res = await fetch('/Chat/Ask', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: msg })
            });
            const data = await res.json();

            renderMsg(data.reply, "bot");

            // Lưu phản hồi của bot
            currentHistory.push({ text: data.reply, sender: "bot" });
            localStorage.setItem("chatHistory", JSON.stringify(currentHistory));
        } catch (e) {
            renderMsg("Hệ thống bận, vui lòng thử lại sau!", "bot");
        }
    }

    // 3. Đăng ký các sự kiện
    btnSend.onclick = send;

    // Cho phép nhấn phím Enter để gửi
    userInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
            send();
        }
    });

    chatToggle.onclick = () => chatBox.classList.toggle("hidden");
    chatClose.onclick = () => chatBox.classList.add("hidden");
    clearBtn.onclick = () => {
        if (confirm("Xóa toàn bộ lịch sử trò chuyện?")) {
            localStorage.removeItem("chatHistory");
            chatMessages.innerHTML = "";
        }
    };
});