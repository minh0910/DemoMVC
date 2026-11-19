// Ajax CRUD modal for Product, Faculty, Major and Student
(function () {
    const modalElement = document.getElementById('ajaxCrudModal');
    if (!modalElement) return;

    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
    const body = modalElement.querySelector('.modal-body');

    async function openModal(url) {
        body.innerHTML = '<div class="text-center py-4"><div class="spinner-border" role="status"></div></div>';
        modal.show();
        try {
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) throw new Error('Không tải được dữ liệu');
            body.innerHTML = await response.text();
        } catch (error) {
            body.innerHTML = '<div class="alert alert-danger">' + error.message + '</div>';
        }
    }

    document.addEventListener('click', function (event) {
        const trigger = event.target.closest('[data-ajax-modal]');
        if (!trigger) return;
        event.preventEventDefault?.();
        event.preventDefault();
        openModal(trigger.dataset.ajaxModal);
    });

    modalElement.addEventListener('submit', async function (event) {
        const form = event.target.closest('form');
        if (!form) return;
        event.preventDefault();

        const submit = form.querySelector('[type="submit"]');
        if (submit) submit.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: form.method || 'POST',
                body: new FormData(form),
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            const contentType = response.headers.get('content-type') || '';
            if (response.ok && contentType.includes('application/json')) {
                const result = await response.json();
                if (result.success) {
                    modal.hide();
                    window.location.reload();
                    return;
                }
                body.innerHTML = '<div class="alert alert-danger">' + (result.message || 'Không thể lưu dữ liệu') + '</div>';
            } else if (response.ok) {
                body.innerHTML = await response.text();
            } else {
                body.innerHTML = '<div class="alert alert-danger">Có lỗi xảy ra khi xử lý dữ liệu.</div>';
            }
        } catch (error) {
            body.innerHTML = '<div class="alert alert-danger">' + error.message + '</div>';
        } finally {
            if (submit) submit.disabled = false;
        }
    });
})();
