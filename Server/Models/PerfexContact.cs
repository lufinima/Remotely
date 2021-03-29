using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Remotely.Server.Models
{
    public class PerfexContact
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userid")]
        public string Userid { get; set; }

        [JsonPropertyName("is_primary")]
        public string IsPrimary { get; set; }

        [JsonPropertyName("firstname")]
        public string Firstname { get; set; }

        [JsonPropertyName("lastname")]
        public string Lastname { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phonenumber")]
        public string Phonenumber { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("datecreated")]
        public string Datecreated { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("new_pass_key")]
        public string NewPassKey { get; set; }

        [JsonPropertyName("new_pass_key_requested")]
        public string NewPassKeyRequested { get; set; }

        [JsonPropertyName("email_verified_at")]
        public string EmailVerifiedAt { get; set; }

        [JsonPropertyName("email_verification_key")]
        public string EmailVerificationKey { get; set; }

        [JsonPropertyName("email_verification_sent_at")]
        public string EmailVerificationSentAt { get; set; }

        [JsonPropertyName("last_ip")]
        public string LastIp { get; set; }

        [JsonPropertyName("last_login")]
        public string LastLogin { get; set; }

        [JsonPropertyName("last_password_change")]
        public string LastPasswordChange { get; set; }

        [JsonPropertyName("active")]
        public string Active { get; set; }

        [JsonPropertyName("profile_image")]
        public string ProfileImage { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("invoice_emails")]
        public string InvoiceEmails { get; set; }

        [JsonPropertyName("estimate_emails")]
        public string EstimateEmails { get; set; }

        [JsonPropertyName("credit_note_emails")]
        public string CreditNoteEmails { get; set; }

        [JsonPropertyName("contract_emails")]
        public string ContractEmails { get; set; }

        [JsonPropertyName("task_emails")]
        public string TaskEmails { get; set; }

        [JsonPropertyName("project_emails")]
        public string ProjectEmails { get; set; }

        [JsonPropertyName("ticket_emails")]
        public string TicketEmails { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("vat")]
        public string Vat { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("zip")]
        public string Zip { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("website")]
        public string Website { get; set; }

        [JsonPropertyName("leadid")]
        public string Leadid { get; set; }

        [JsonPropertyName("billing_street")]
        public string BillingStreet { get; set; }

        [JsonPropertyName("billing_city")]
        public string BillingCity { get; set; }

        [JsonPropertyName("billing_state")]
        public string BillingState { get; set; }

        [JsonPropertyName("billing_zip")]
        public string BillingZip { get; set; }

        [JsonPropertyName("billing_country")]
        public string BillingCountry { get; set; }

        [JsonPropertyName("shipping_street")]
        public string ShippingStreet { get; set; }

        [JsonPropertyName("shipping_city")]
        public string ShippingCity { get; set; }

        [JsonPropertyName("shipping_state")]
        public string ShippingState { get; set; }

        [JsonPropertyName("shipping_zip")]
        public string ShippingZip { get; set; }

        [JsonPropertyName("shipping_country")]
        public string ShippingCountry { get; set; }

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }

        [JsonPropertyName("default_language")]
        public string DefaultLanguage { get; set; }

        [JsonPropertyName("default_currency")]
        public string DefaultCurrency { get; set; }

        [JsonPropertyName("show_primary_contact")]
        public string ShowPrimaryContact { get; set; }

        [JsonPropertyName("stripe_id")]
        public string StripeId { get; set; }

        [JsonPropertyName("registration_confirmed")]
        public string RegistrationConfirmed { get; set; }

        [JsonPropertyName("addedfrom")]
        public string Addedfrom { get; set; }
    }
}
