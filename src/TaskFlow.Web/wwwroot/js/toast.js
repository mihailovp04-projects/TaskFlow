// Модуль всплывающих уведомлений. Загружается из .NET через import("./js/toast.js")
const CONTAINER_ID = 'tf-toast-container';

function getContainer() {
    let container = document.getElementById(CONTAINER_ID);
    if (!container) {
        container = document.createElement('div');
        container.id = CONTAINER_ID;
        container.className = 'tf-toast-container';
        container.setAttribute('aria-live', 'polite');
        document.body.appendChild(container);
    }
    return container;
}

// id - идентификатор уведомления, kind - info|success|warning|error,
// actionText - текст кнопки действия (или null), dotNetRef - DotNetObjectReference (или null)
export function showToast(id, message, kind, actionText, durationMs, dotNetRef) {
    const toast = document.createElement('div');
    toast.className = `tf-toast tf-toast-${kind}`;
    toast.setAttribute('role', kind === 'error' ? 'alert' : 'status');

    const text = document.createElement('span');
    text.className = 'tf-toast-text';
    // textContent, а не innerHTML: текст не интерпретируется как HTML (защита от XSS)
    text.textContent = message;
    toast.appendChild(text);

    let closed = false;
    const close = (notifyDotNet) => {
        if (closed) {
            return;
        }
        closed = true;
        toast.remove();
        if (notifyDotNet && dotNetRef) {
            // Сообщаем .NET, что действие больше не понадобится
            dotNetRef.invokeMethodAsync('OnToastExpired', id);
        }
    };

    if (actionText && dotNetRef) {
        const actionButton = document.createElement('button');
        actionButton.type = 'button';
        actionButton.className = 'tf-toast-action';
        actionButton.textContent = actionText;
        actionButton.addEventListener('click', async () => {
            close(false);
            // Вызов метода экземпляра .NET, помеченного [JSInvokable]
            await dotNetRef.invokeMethodAsync('OnToastAction', id);
        });
        toast.appendChild(actionButton);
    }

    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'tf-toast-close';
    closeButton.setAttribute('aria-label', 'Close');
    closeButton.textContent = '×';
    closeButton.addEventListener('click', () => close(true));
    toast.appendChild(closeButton);

    getContainer().appendChild(toast);
    setTimeout(() => close(true), durationMs);
}

export function clearToasts() {
    const container = document.getElementById(CONTAINER_ID);
    if (container) {
        container.replaceChildren();
    }
}
