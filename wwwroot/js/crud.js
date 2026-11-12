// Buổi 11: CRUD bằng Modal + Ajax (jQuery), dùng chung cho mọi trang có #tableContainer
(function ($) {
    const $modal = $('#crudModal');
    if (!$modal.length) return;
    const modal = bootstrap.Modal.getOrCreateInstance($modal[0]);
    const confirmModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('confirmModal'));
    const token = () => $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();

    // Thông báo nhỏ ở góc phải màn hình
    window.showToast = function (message, ok = true) {
        const $t = $(`<div class="toast align-items-center text-bg-${ok ? 'success' : 'danger'} border-0" role="alert">
            <div class="d-flex"><div class="toast-body"></div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Đóng"></button></div></div>`);
        $t.find('.toast-body').text(message);
        $('#ajaxToasts').append($t);
        $t.on('hidden.bs.toast', () => $t.remove());
        new bootstrap.Toast($t[0], { delay: 4000 }).show();
    };

    function errorMessage(xhr, fallback) {
        if (xhr.status === 404) return 'Không tìm thấy dữ liệu';
        return fallback;
    }

    // Tải lại bảng dữ liệu
    // Bài 12: trang đang xem
    let currentPage = 1;

    window.reloadTable = function () {
        const $c = $('#tableContainer');
        // Gửi kèm các ô của #filterForm (keyword, pageSize, bộ lọc) và trang hiện tại
        return $.get($c.data('url'), $('#filterForm').serialize() + '&page=' + currentPage)
            .done(html => $c.html(html))
            .fail(xhr => $c.html($('<div class="alert alert-danger"></div>').text(errorMessage(xhr, 'Không tải được dữ liệu'))));
    };

    // Form nạp bằng Ajax chưa có validation phía client, cần parse lại
    function parseForm() {
        const $form = $modal.find('form');
        if ($form.length && $.validator && $.validator.unobtrusive) {
            $form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse($form);
        }
        $modal.trigger('form-loaded');
    }

    // Mở modal: Thêm / Sửa / Chi tiết
    $(document).on('click', '[data-modal]', function (e) {
        e.preventDefault();
        $.get($(this).data('modal'))
            .done(html => {
                $modal.find('.modal-content').html(html);
                parseForm();
                modal.show();
            })
            .fail(xhr => showToast(errorMessage(xhr, 'Có lỗi xảy ra'), false));
    });

    // Lưu form trong modal
    $(document).on('submit', '#crudModal form.ajax-form', function (e) {
        e.preventDefault();
        const $form = $(this);
        if ($form.valid && !$form.valid()) return;
        const $btn = $form.find('[type=submit]').prop('disabled', true);
        $.ajax({ url: $form.attr('action'), type: 'POST', data: $form.serialize() })
            .done(res => {
                showToast(res.message, res.success);
                if (res.success) {
                    modal.hide();
                    reloadTable();
                }
            })
            .fail(xhr => {
                if (xhr.status === 400 && xhr.responseText) {
                    // Lỗi validation phía server: hiện lại form kèm thông báo lỗi
                    $modal.find('.modal-content').html(xhr.responseText);
                    parseForm();
                } else {
                    showToast(errorMessage(xhr, 'Có lỗi xảy ra khi lưu'), false);
                }
            })
            .always(() => $btn.prop('disabled', false));
    });

    // Xoá: hỏi lại trong modal xác nhận rồi gửi POST
    let deleteUrl = null;
    $(document).on('click', '[data-delete]', function (e) {
        e.preventDefault();
        deleteUrl = $(this).data('delete');
        $('#confirmText').text(`Bạn có chắc muốn xoá ${$(this).data('name') || 'bản ghi này'}?`);
        bootstrap.Modal.getInstance($modal[0])?.hide();
        confirmModal.show();
    });
    $('#confirmOk').on('click', function () {
        if (!deleteUrl) return;
        $.post(deleteUrl, { __RequestVerificationToken: token() })
            .done(res => {
                showToast(res.message, res.success);
                if (res.success) reloadTable();
            })
            .fail(xhr => showToast(errorMessage(xhr, 'Có lỗi xảy ra khi xoá'), false))
            .always(() => { confirmModal.hide(); deleteUrl = null; });
    });

    // Bài 12: bấm nút trang
    $(document).on('click', '#tableContainer [data-page]', function (e) {
        e.preventDefault();
        const $li = $(this).closest('.page-item');
        if ($li.hasClass('disabled') || $li.hasClass('active')) return;
        currentPage = parseInt($(this).data('page'), 10);
        reloadTable();
    });

    // Bài 12: đổi điều kiện tìm/lọc thì về trang 1; gõ phím dừng 400ms thì tự tìm
    function timKiem() { currentPage = 1; reloadTable(); }
    let hen;
    $('#filterForm').on('submit', e => { e.preventDefault(); timKiem(); });
    $('#filterForm select').on('change', timKiem);
    $('#filterForm input[type=search]').on('input', () => { clearTimeout(hen); hen = setTimeout(timKiem, 400); });

    if ($('#tableContainer').length) reloadTable();
})(jQuery);
