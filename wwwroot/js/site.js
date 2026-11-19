// Ajax CRUD modal for Product, Faculty, Major and Student.
(function () {
    const modalElement = document.getElementById('ajaxCrudModal');
    if (!modalElement) return;

    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
    const body = modalElement.querySelector('.modal-body');
    const title = modalElement.querySelector('.modal-title');

    function initializeStudentForm() {
        const form = body.querySelector('form[data-majors]');
        if (!form) return;

        let majors;
        try { majors = JSON.parse(form.dataset.majors || '[]'); }
        catch { majors = []; }

        const faculty = form.querySelector('[name="FacultyCode"]');
        const major = form.querySelector('[name="MajorCode"]');
        const year = form.querySelector('[name="EnrollmentYear"]');
        const system = form.querySelector('[name="TrainingSystemCode"]');
        const preview = form.querySelector('#codePreview');
        if (!faculty || !major || !year || !system || !preview) return;

        function fillMajors(selected) {
            major.replaceChildren(new Option(faculty.value ? '-- Chọn ngành --' : '-- Chọn khoa trước --', ''));
            majors.filter(item => item.facultyCode === faculty.value).forEach(item => {
                major.add(new Option(item.code + ' - ' + item.name, item.code, false, item.code === selected));
            });
        }

        let requestId = 0;
        async function updatePreview() {
            preview.value = '';
            if (!year.value || !system.value || !major.value) return;
            const currentRequest = ++requestId;
            const query = new URLSearchParams({ enrollmentYear: year.value, trainingSystemCode: system.value, majorCode: major.value });
            try {
                const response = await fetch(form.dataset.nextCodeUrl + '?' + query);
                if (!response.ok || currentRequest !== requestId) return;
                const result = await response.json();
                if (currentRequest === requestId) preview.value = result.code || 'Không sinh được mã';
            } catch { preview.value = ''; }
        }

        faculty.addEventListener('change', () => { fillMajors(''); updatePreview(); });
        major.addEventListener('change', updatePreview);
        system.addEventListener('change', updatePreview);
        year.addEventListener('input', updatePreview);
        fillMajors(major.dataset.selected || '');
        updatePreview();
    }

    function setModalContent(html) {
        body.innerHTML = html;
        const form = body.querySelector('form[data-modal-title]');
        if (form) title.textContent = form.dataset.modalTitle;
        initializeStudentForm();
    }

    async function openModal(url) {
        body.innerHTML = '<div class="text-center py-4"><div class="spinner-border" role="status" aria-label="Đang tải"></div></div>';
        title.textContent = 'Đang tải…';
        modal.show();
        try {
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) throw new Error('Không tải được dữ liệu. Vui lòng thử lại.');
            setModalContent(await response.text());
        } catch (error) {
            body.innerHTML = '<div class="alert alert-danger"></div>';
            body.querySelector('.alert').textContent = error.message;
            title.textContent = 'Có lỗi';
        }
    }

    document.addEventListener('click', function (event) {
        const trigger = event.target.closest('[data-ajax-modal]');
        if (!trigger) return;
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
            if (contentType.includes('application/json')) {
                const result = await response.json();
                if (response.ok && result.success) {
                    modal.hide();
                    window.location.reload();
                    return;
                }
                const alert = document.createElement('div');
                alert.className = 'alert alert-danger';
                alert.textContent = result.message || 'Không thể lưu dữ liệu.';
                body.prepend(alert);
            } else if (response.ok) {
                setModalContent(await response.text());
            } else {
                body.innerHTML = '<div class="alert alert-danger">Có lỗi xảy ra khi xử lý dữ liệu.</div>';
            }
        } catch (error) {
            body.innerHTML = '<div class="alert alert-danger"></div>';
            body.querySelector('.alert').textContent = error.message;
        } finally {
            if (submit && submit.isConnected) submit.disabled = false;
        }
    });
})();
