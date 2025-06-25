/**
 * ملف JavaScript محسن لإدارة المهام مع النموذج المنبثق
 * Enhanced Task Management with Modal Integration
 */

class TaskManager {
    constructor() {
        this.isEditMode = false;
        this.currentTaskId = 0;
        this.employees = [];
        this.init();
    }

    /**
     * تهيئة النظام
     */
    init() {
        this.bindEvents();
        this.setupModal();
        this.loadEmployees();
    }

    /**
     * ربط الأحداث
     */
    bindEvents() {
        // إغلاق النماذج عند النقر خارجها
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal-overlay')) {
                this.closeModal();
            }
        });

        // اختصارات لوحة المفاتيح
        document.addEventListener('keydown', (e) => this.handleKeyboardShortcuts(e));

        // نموذج المهمة
        const taskForm = document.getElementById('taskForm');
        if (taskForm) {
            taskForm.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }
    }

    /**
     * إعداد النموذج المنبثق
     */
    setupModal() {
        // إضافة تأثيرات الانتقال
        const modal = document.getElementById('taskModal');
        if (modal) {
            modal.style.transition = 'all 0.3s ease';
        }
    }

    /**
     * تحميل قائمة الموظفين من الخادم
     */
    async loadEmployees() {
        try {
            const response = await fetch('/Task/GetEmployees', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();

                if (result.success) {
                    this.employees = result.data;
                    this.populateEmployeeSelect();
                } else {
                    console.error('فشل في جلب قائمة الموظفين:', result.message);
                    this.showToast('خطأ', result.message || 'فشل في جلب قائمة الموظفين', 'error');
                    // استخدام بيانات احتياطية
                    this.loadFallbackEmployees();
                }
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('خطأ في تحميل قائمة الموظفين:', error);
            this.showToast('خطأ', 'حدث خطأ في الاتصال بالخادم', 'error');
            // استخدام بيانات احتياطية
            this.loadFallbackEmployees();
        }
    }

    /**
     * تحميل بيانات احتياطية للموظفين
     */
    loadFallbackEmployees() {
        this.employees = [
            { value: "1", text: "أحمد محمد", role: "Manager" },
            { value: "2", text: "فاطمة علي", role: "Employee" },
            { value: "3", text: "محمد حسن", role: "Employee" }
        ];
        this.populateEmployeeSelect();
    }

    /**
     * ملء قائمة الموظفين في النموذج
     */
    populateEmployeeSelect() {
        const select = document.getElementById('taskAssignedTo');
        if (!select) return;

        // مسح الخيارات الحالية
        select.innerHTML = '<option value="">-- اختر موظفًا --</option>';

        // إضافة الموظفين
        this.employees.forEach(emp => {
            const option = document.createElement('option');
            option.value = emp.value;
            option.textContent = emp.text;
            option.dataset.role = emp.role || 'Employee';
            select.appendChild(option);
        });
    }

    /**
     * فتح نموذج الإنشاء
     */
    openCreateModal() {
        this.isEditMode = false;
        this.currentTaskId = 0;

        // تحديث عناصر النموذج
        document.getElementById('modalTitle').textContent = 'مهمة جديدة';
        document.getElementById('submitText').textContent = 'إنشاء المهمة';

        // إخفاء حقل الحالة للمهام الجديدة
        const statusField = document.getElementById('statusField');
        if (statusField) {
            statusField.style.display = 'none';
        }

        // إعادة تعيين النموذج
        this.resetForm();

        // إظهار النموذج
        this.showModal();

        // التأكد من تحميل قائمة الموظفين
        if (this.employees.length === 0) {
            this.loadEmployees();
        }
    }

    /**
     * فتح نموذج التعديل
     */
    async openEditModal(taskId) {
        this.isEditMode = true;
        this.currentTaskId = taskId;

        // تحديث عناصر النموذج
        document.getElementById('modalTitle').textContent = 'تعديل مهمة';
        document.getElementById('submitText').textContent = 'حفظ التعديلات';

        // إظهار حقل الحالة للتعديل
        const statusField = document.getElementById('statusField');
        if (statusField) {
            statusField.style.display = 'block';
        }

        // مسح الأخطاء
        this.clearErrors();

        try {
            // إظهار حالة التحميل
            this.showLoadingState();

            // تحميل بيانات المهمة
            await this.loadTaskData(taskId);

            // إظهار النموذج
            this.showModal();

            // التأكد من تحميل قائمة الموظفين
            if (this.employees.length === 0) {
                await this.loadEmployees();
            }
        } catch (error) {
            console.error('خطأ في فتح نموذج التعديل:', error);
            this.showToast('خطأ', 'حدث خطأ في تحميل بيانات المهمة', 'error');
        } finally {
            this.hideLoadingState();
        }
    }

    /**
     * تحميل بيانات المهمة للتعديل
     */
    async loadTaskData(taskId) {
        try {
            const response = await fetch(`/Task/GetTask/${taskId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();

                if (result.success) {
                    this.populateForm(result.data);
                } else {
                    throw new Error(result.message || 'فشل في جلب بيانات المهمة');
                }
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('خطأ في تحميل بيانات المهمة:', error);
            throw error;
        }
    }

    /**
     * ملء النموذج بالبيانات
     */
    populateForm(taskData) {
        document.getElementById('taskId').value = taskData.id || 0;
        document.getElementById('taskTitle').value = taskData.title || '';
        document.getElementById('taskPriority').value = taskData.priority || '';
        document.getElementById('taskDescription').value = taskData.description || '';
        document.getElementById('taskSystem').value = taskData.system || '';

        // تنسيق التاريخ
        if (taskData.dueDate) {
            const date = new Date(taskData.dueDate);
            const formattedDate = date.toISOString().split('T')[0];
            document.getElementById('taskDueDate').value = formattedDate;
        }

        // تعيين الحالة (للتعديل فقط)
        const statusSelect = document.getElementById('taskStatus');
        if (statusSelect && taskData.status) {
            statusSelect.value = taskData.status;
        }

        // تعيين الموظف المسؤول
        const assignedToSelect = document.getElementById('taskAssignedTo');
        if (assignedToSelect && taskData.assignedToUserId) {
            assignedToSelect.value = taskData.assignedToUserId.toString();
        }
    }

    /**
     * إعادة تعيين النموذج
     */
    resetForm() {
        const form = document.getElementById('taskForm');
        if (form) {
            form.reset();
        }

        document.getElementById('taskId').value = '0';
        this.clearErrors();
    }

    /**
     * إظهار النموذج المنبثق
     */
    showModal() {
        const modal = document.getElementById('taskModal');
        if (modal) {
            modal.classList.add('active');

            // التركيز على أول حقل
            setTimeout(() => {
                const firstInput = document.getElementById('taskTitle');
                if (firstInput) {
                    firstInput.focus();
                }
            }, 300);
        }
    }

    /**
     * إخفاء النموذج المنبثق
     */
    closeModal() {
        const modal = document.getElementById('taskModal');
        if (modal) {
            modal.classList.remove('active');
        }

        // إعادة تعيين النموذج بعد إغلاقه
        setTimeout(() => {
            this.resetForm();
        }, 300);
    }

    /**
     * معالجة إرسال النموذج
     */
    async handleFormSubmit(e) {
        e.preventDefault();

        // التحقق من صحة البيانات
        if (!this.validateForm()) {
            return;
        }

        // الحصول على بيانات النموذج
        const formData = this.getFormData();

        try {
            // إظهار حالة التحميل
            this.showLoadingState();

            // تحديد الـ URL والطريقة
            const url = this.isEditMode ? `/Task/EditAjax/${this.currentTaskId}` : '/Task/CreateAjax';

            // إرسال الطلب
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                const result = await response.json();

                if (result.success) {
                    // إظهار رسالة النجاح
                    this.showToast(
                        'نجح!',
                        this.isEditMode ? 'تم حفظ التعديلات بنجاح' : 'تم إنشاء المهمة بنجاح',
                        'success'
                    );

                    // إغلاق النموذج
                    this.closeModal();

                    // إعادة تحميل الصفحة
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    // عرض أخطاء التحقق
                    if (result.errors) {
                        this.displayErrors(result.errors);
                    } else {
                        this.showToast('خطأ', result.message || 'حدث خطأ أثناء حفظ البيانات', 'error');
                    }
                }
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('خطأ في إرسال النموذج:', error);
            this.showToast('خطأ', 'حدث خطأ في الاتصال بالخادم', 'error');
        } finally {
            this.hideLoadingState();
        }
    }

    /**
     * التحقق من صحة النموذج
     */
    validateForm() {
        this.clearErrors();
        let isValid = true;

        // التحقق من العنوان
        const title = document.getElementById('taskTitle').value.trim();
        if (!title) {
            this.showFieldError('titleError', 'عنوان المهمة مطلوب');
            isValid = false;
        }

        // التحقق من الأولوية
        const priority = document.getElementById('taskPriority').value;
        if (!priority) {
            this.showFieldError('priorityError', 'الأولوية مطلوبة');
            isValid = false;
        }

        // التحقق من الموظف المسؤول
        const assignedTo = document.getElementById('taskAssignedTo').value;
        if (!assignedTo) {
            this.showFieldError('assignedToError', 'يجب تعيين المهمة لموظف');
            isValid = false;
        }

        return isValid;
    }

    /**
     * الحصول على بيانات النموذج
     */
    getFormData() {
        const formData = {
            title: document.getElementById('taskTitle').value.trim(),
            priority: document.getElementById('taskPriority').value,
            description: document.getElementById('taskDescription').value.trim(),
            system: document.getElementById('taskSystem').value.trim(),
            assignedToUserId: parseInt(document.getElementById('taskAssignedTo').value) || 0
        };

        // إضافة التاريخ إذا كان موجوداً
        const dueDate = document.getElementById('taskDueDate').value;
        if (dueDate) {
            formData.dueDate = dueDate;
        }

        // إضافة الحالة للتعديل
        if (this.isEditMode) {
            formData.id = this.currentTaskId;
            formData.status = document.getElementById('taskStatus').value;
        }

        return formData;
    }

    /**
     * عرض أخطاء الحقول
     */
    showFieldError(elementId, message) {
        const errorElement = document.getElementById(elementId);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        }
    }

    /**
     * مسح الأخطاء
     */
    clearErrors() {
        const errorElements = document.querySelectorAll('[id$="Error"]');
        errorElements.forEach(element => {
            element.textContent = '';
            element.style.display = 'none';
        });
    }

    /**
     * عرض أخطاء الخادم
     */
    displayErrors(errors) {
        Object.keys(errors).forEach(field => {
            const errorElementId = field.toLowerCase() + 'Error';
            const errorElement = document.getElementById(errorElementId);
            if (errorElement && errors[field] && errors[field].length > 0) {
                this.showFieldError(errorElementId, errors[field][0]);
            }
        });
    }

    /**
     * عرض حالة التحميل
     */
    showLoadingState() {
        const submitBtn = document.getElementById('submitBtn');
        const submitText = document.getElementById('submitText');
        const submitSpinner = document.getElementById('submitSpinner');

        if (submitBtn) {
            submitBtn.disabled = true;
        }

        if (submitSpinner) {
            submitSpinner.style.display = 'inline-block';
        }

        if (submitText) {
            submitText.textContent = this.isEditMode ? 'جاري الحفظ...' : 'جاري الإنشاء...';
        }
    }

    /**
     * إخفاء حالة التحميل
     */
    hideLoadingState() {
        const submitBtn = document.getElementById('submitBtn');
        const submitText = document.getElementById('submitText');
        const submitSpinner = document.getElementById('submitSpinner');

        if (submitBtn) {
            submitBtn.disabled = false;
        }

        if (submitSpinner) {
            submitSpinner.style.display = 'none';
        }

        if (submitText) {
            submitText.textContent = this.isEditMode ? 'حفظ التعديلات' : 'إنشاء المهمة';
        }
    }

    /**
     * معالجة اختصارات لوحة المفاتيح
     */
    handleKeyboardShortcuts(e) {
        // Escape لإغلاق النموذج
        if (e.key === 'Escape') {
            this.closeModal();
        }

        // Ctrl/Cmd + Enter لإرسال النموذج
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            const modal = document.getElementById('taskModal');
            if (modal && modal.classList.contains('active')) {
                e.preventDefault();
                const form = document.getElementById('taskForm');
                if (form) {
                    form.dispatchEvent(new Event('submit', { cancelable: true }));
                }
            }
        }
    }

    /**
     * عرض رسالة Toast
     */
    showToast(title, message, type = 'info') {
        // استخدام SweetAlert2 إذا كان متوفراً
        if (typeof Swal !== 'undefined') {
            const iconMap = {
                success: 'success',
                error: 'error',
                warning: 'warning',
                info: 'info'
            };

            Swal.fire({
                title: title,
                text: message,
                icon: iconMap[type] || 'info',
                confirmButtonText: 'موافق',
                timer: type === 'success' ? 3000 : undefined,
                timerProgressBar: type === 'success'
            });
        } else {
            // fallback للمتصفحات التي لا تدعم SweetAlert2
            alert(`${title}: ${message}`);
        }
    }

    /**
     * الحصول على رمز الحماية
     */
    getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    /**
     * فلترة المهام (للبحث والفلترة)
     */
    filterTasks() {
        const searchTerm = document.getElementById('searchInput')?.value.toLowerCase() || '';
        const priorityFilter = document.getElementById('priorityFilter')?.value || '';
        const statusFilter = document.getElementById('statusFilter')?.value || '';

        const taskCards = document.querySelectorAll('.task-card');

        taskCards.forEach(card => {
            const title = card.dataset.title || '';
            const priority = card.dataset.priority || '';
            const status = card.dataset.status || '';

            const matchesSearch = title.includes(searchTerm);
            const matchesPriority = !priorityFilter || priority === priorityFilter;
            const matchesStatus = !statusFilter || status === statusFilter;

            if (matchesSearch && matchesPriority && matchesStatus) {
                card.style.display = '';
                card.style.animation = 'fadeIn 0.3s ease';
            } else {
                card.style.display = 'none';
            }
        });

        // تحديث الإحصائيات
        this.updateStats();
    }

    /**
     * تحديث الإحصائيات
     */
    updateStats() {
        const visibleCards = document.querySelectorAll('.task-card[style=""], .task-card:not([style])');
        const totalTasks = visibleCards.length;

        let completedTasks = 0;
        let overdueTasks = 0;
        let pendingTasks = 0;

        visibleCards.forEach(card => {
            const status = card.dataset.status;
            if (status === 'Completed') {
                completedTasks++;
            } else if (status === 'Overdue') {
                overdueTasks++;
            } else {
                pendingTasks++;
            }
        });

        // تحديث العناصر في الصفحة
        const totalElement = document.getElementById('totalTasks');
        const completedElement = document.getElementById('completedTasks');
        const overdueElement = document.getElementById('overdueTasks');
        const pendingElement = document.getElementById('pendingTasks');

        if (totalElement) totalElement.textContent = totalTasks;
        if (completedElement) completedElement.textContent = completedTasks;
        if (overdueElement) overdueElement.textContent = overdueTasks;
        if (pendingElement) pendingElement.textContent = pendingTasks;
    }
}

// دوال عامة للاستخدام في HTML
function openCreateModal() {
    if (window.taskManager) {
        window.taskManager.openCreateModal();
    }
}

function openEditModal(taskId) {
    if (window.taskManager) {
        window.taskManager.openEditModal(taskId);
    }
}

function closeModal() {
    if (window.taskManager) {
        window.taskManager.closeModal();
    }
}

function filterTasks() {
    if (window.taskManager) {
        window.taskManager.filterTasks();
    }
}

function confirmDelete(taskId, taskTitle) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'هل أنت متأكد؟',
            text: `سيتم حذف المهمة "${taskTitle}" نهائيًا!`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'نعم، احذف',
            cancelButtonText: 'إلغاء',
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                deleteTask(taskId, taskTitle);
            }
        });
    } else {
        if (confirm(`هل أنت متأكد من حذف المهمة "${taskTitle}"؟`)) {
            deleteTask(taskId, taskTitle);
        }
    }
}

async function deleteTask(taskId, taskTitle) {
    try {
        const response = await fetch(`/Task/DeleteConfirmed/${taskId}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': window.taskManager?.getAntiForgeryToken() || ''
            }
        });

        if (response.ok) {
            const result = await response.json();

            if (result.success) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'تم الحذف!',
                        text: `تم حذف المهمة "${taskTitle}" بنجاح.`,
                        icon: 'success',
                        confirmButtonText: 'موافق'
                    }).then(() => {
                        window.location.reload();
                    });
                } else {
                    alert(`تم حذف المهمة "${taskTitle}" بنجاح.`);
                    window.location.reload();
                }
            } else {
                throw new Error(result.message || 'فشل في حذف المهمة');
            }
        } else {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
    } catch (error) {
        console.error('خطأ في حذف المهمة:', error);

        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'خطأ!',
                text: 'حدث خطأ أثناء حذف المهمة.',
                icon: 'error',
                confirmButtonText: 'موافق'
            });
        } else {
            alert('حدث خطأ أثناء حذف المهمة.');
        }
    }
}

// تهيئة النظام عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', function () {
    window.taskManager = new TaskManager();

    // إضافة CSS للتأثيرات
    const style = document.createElement('style');
    style.textContent = `
        .modal-overlay {
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s ease;
        }
        
        .modal-overlay.active {
            opacity: 1;
            visibility: visible;
        }
        
        .modal-container {
            transform: scale(0.9) translateY(20px);
            transition: all 0.3s ease;
        }
        
        .modal-overlay.active .modal-container {
            transform: scale(1) translateY(0);
        }
        
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }
        
        .loading-spinner {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }
        
        .spinner {
            width: 16px;
            height: 16px;
            border: 2px solid #e5e7eb;
            border-top: 2px solid #3b82f6;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
    `;
    document.head.appendChild(style);
});

