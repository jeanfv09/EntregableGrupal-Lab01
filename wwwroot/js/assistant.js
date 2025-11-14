// =======================================================
//   REFERENCIAS A ELEMENTOS
// =======================================================
const btn = document.getElementById("assistant-button");
const win = document.getElementById("assistant-window");
const msgBox = document.getElementById("assistant-messages");
const input = document.getElementById("assistant-input");
const sendBtn = document.getElementById("assistant-send");

// =======================================================
//   ABRIR / CERRAR VENTANA
// =======================================================
btn.addEventListener("click", () => {
    win.style.display = win.style.display === "flex" ? "none" : "flex";
});

// =======================================================
//   FUNCIÓN PARA AGREGAR BURBUJAS
// =======================================================
function addMessage(text, type) {
    const div = document.createElement("div");
    div.classList.add(type === "user" ? "msg-user" : "msg-bot");
    div.textContent = text;
    msgBox.appendChild(div);
    msgBox.scrollTop = msgBox.scrollHeight;
}

// =======================================================
//   FUNCIÓN PRINCIPAL DE ENVÍO (MEJORADA)
// =======================================================
async function sendMessage() {
    const text = input.value.trim();
    if (!text) return;

    addMessage(text, "user");
    input.value = "";
    
    // 🔥 NUEVO: Mostrar indicador de "escribiendo"
    const typingIndicator = document.createElement("div");
    typingIndicator.id = "typing-indicator";
    typingIndicator.classList.add("msg-bot");
    typingIndicator.innerHTML = "MedAssist está escribiendo... <i class='bi bi-three-dots'></i>";
    msgBox.appendChild(typingIndicator);
    msgBox.scrollTop = msgBox.scrollHeight;

    try {
        const response = await fetch("/assistant/ask", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                message: text,
                page: document.title
            })
        });

        // 🔥 NUEVO: Remover indicador de escritura
        typingIndicator.remove();

        const data = await response.json();
        addMessage(data.reply, "bot");

    } catch (err) {
        typingIndicator.remove();
        addMessage("❌ Error al conectar con el asistente.", "bot");
    }
}

// =======================================================
//   EVENTOS DE ENVÍO (CLICK + ENTER)
// =======================================================
sendBtn.addEventListener("click", sendMessage);

input.addEventListener("keypress", function (e) {
    if (e.key === "Enter") sendMessage();
});

// =======================================================
//   GUARDAR HISTORIAL EN sessionStorage
// =======================================================
window.addEventListener("beforeunload", () => {
    sessionStorage.setItem("assistantHistory", msgBox.innerHTML);
});

// =======================================================
//   RESTAURAR HISTORIAL AL ABRIR LA PÁGINA
// =======================================================
window.addEventListener("load", () => {
    const saved = sessionStorage.getItem("assistantHistory");
    if (saved) msgBox.innerHTML = saved;
});
