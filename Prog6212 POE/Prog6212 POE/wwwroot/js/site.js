// Enhanced auto-calculation and validation
$(document).ready(function () {
    // Real-time calculation with AJAX
    $('#HoursWorked, #Rate').on('input', function () {
        calculateAmount();
        validateInputs();
        updateProgressBar();
    });

    // Contract change handler
    $('#Contract').on('change', function () {
        updateRateBasedOnContract();
    });

    function calculateAmount() {
        const hours = parseInt($('#HoursWorked').val()) || 0;
        const rate = parseFloat($('#Rate').val()) || 0;
        const amount = hours * rate;

        $('#calculatedAmount').val(amount.toFixed(2));
        $('#amountDisplay').text('R ' + amount.toFixed(2));

        // Update hidden field if exists
        $('input[name="Amount"]').val(amount);
    }

    function validateInputs() {
        const hours = parseInt($('#HoursWorked').val()) || 0;
        const rate = parseFloat($('#Rate').val()) || 0;
        const contract = $('#Contract').val();

        let isValid = true;

        // Clear previous errors
        $('.validation-error').remove();

        // Validation rules
        if (hours > 12) {
            showError('HoursWorked', 'Maximum 12 hours allowed per claim');
            isValid = false;
        }

        if (rate > 500) {
            showError('Rate', 'Maximum hourly rate is R500');
            isValid = false;
        }

        if (hours <= 0) {
            showError('HoursWorked', 'Hours worked must be greater than 0');
            isValid = false;
        }

        if (rate <= 0) {
            showError('Rate', 'Hourly rate must be greater than 0');
            isValid = false;
        }

        $('#submitBtn').prop('disabled', !isValid);
        return isValid;
    }

    function showError(fieldId, message) {
        $('#' + fieldId).after('<div class="validation-error text-danger small">' + message + '</div>');
    }

    function updateRateBasedOnContract() {
        const contract = $('#Contract').val();
        let suggestedRate = 0;

        // Predefined rates based on contract type
        switch (contract) {
            case 'Prog-2025':
                suggestedRate = 350;
                break;
            case 'Research-2024':
                suggestedRate = 400;
                break;
            default:
                suggestedRate = 300;
        }

        if ($('#Rate').val() === '' || $('#Rate').val() === '0') {
            $('#Rate').val(suggestedRate);
            calculateAmount();
        }
    }

    function updateProgressBar() {
        const hours = parseInt($('#HoursWorked').val()) || 0;
        const progress = Math.min((hours / 12) * 100, 100);
        $('#hoursProgress .progress-bar').css('width', progress + '%');
    }

    // Initialize on page load
    updateRateBasedOnContract();
    calculateAmount();
    validateInputs();
});

// File upload automation
function handleFileUpload(input) {
    const maxSize = 5 * 1024 * 1024; // 5MB
    const file = input.files[0];

    if (file) {
        // Validate file size
        if (file.size > maxSize) {
            alert('File size must be less than 5MB');
            input.value = '';
            return false;
        }

        // Validate file type
        const allowedTypes = ['application/pdf',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'];
        if (!allowedTypes.includes(file.type)) {
            alert('Please select a PDF, DOCX, or XLSX file');
            input.value = '';
            return false;
        }

        // Show file info
        $('#fileInfo').html(`
            <div class="alert alert-info">
                <i class="fas fa-file"></i> ${file.name} 
                <br><small>Size: ${(file.size / 1024 / 1024).toFixed(2)} MB</small>
            </div>
        `);

        return true;
    }

    return false;
}
