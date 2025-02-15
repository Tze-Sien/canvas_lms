namespace CanvasLMS.Models 
{
     public enum Role
    {
        Student,
        Lecturer,
        Admin,
        FacultyAdmin
    }

    public enum StudentStatus
    {
        Active,
        Inactive,
        Graduated
    }

    public enum AddDropApproval
    {
        Pending,
        Approved,
        Rejected
    }

    public enum EnrollmentStatus
    {
        Enrolled,
        Dropped
    }

    public enum PaymentStatus
    {
        Paid,
        Pending,
        Failed
    }

    public enum InvoiceStatus
    {
        Paid,
        Pending,
        Void
    }
}