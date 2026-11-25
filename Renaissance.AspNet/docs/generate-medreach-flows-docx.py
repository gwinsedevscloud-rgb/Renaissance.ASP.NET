"""Generate MedReach Operational Flows Word document."""
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor

OUT = Path(__file__).resolve().parent / "MedReach-Operational-Flows.docx"


def set_run_font(run, size=11, bold=False, color=None):
    run.font.name = "Calibri"
    run._element.rPr.rFonts.set(qn("w:eastAsia"), "Calibri")
    run.font.size = Pt(size)
    run.bold = bold
    if color:
        run.font.color.rgb = color


def add_heading(doc, text, level=1):
    h = doc.add_heading(text, level=level)
    for run in h.runs:
        run.font.color.rgb = RGBColor(0x00, 0x7F, 0x7F)
    return h


def add_para(doc, text, bold=False, italic=False):
    p = doc.add_paragraph()
    run = p.add_run(text)
    set_run_font(run, bold=bold)
    run.italic = italic
    p.paragraph_format.space_after = Pt(6)
    return p


def add_bullets(doc, items, numbered=False):
    style = "List Number" if numbered else "List Bullet"
    for item in items:
        p = doc.add_paragraph(item, style=style)
        p.paragraph_format.space_after = Pt(2)


def add_table(doc, headers, rows):
    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.style = "Table Grid"
    hdr = table.rows[0].cells
    for i, h in enumerate(headers):
        hdr[i].text = h
        for p in hdr[i].paragraphs:
            for run in p.runs:
                set_run_font(run, size=10, bold=True, color=RGBColor(0x00, 0x7F, 0x7F))
    for r_i, row in enumerate(rows):
        cells = table.rows[r_i + 1].cells
        for c_i, val in enumerate(row):
            cells[c_i].text = str(val)
            for p in cells[c_i].paragraphs:
                for run in p.runs:
                    set_run_font(run, size=10)
    doc.add_paragraph()


def add_flow_box(doc, lines):
    p = doc.add_paragraph()
    run = p.add_run("\n".join(lines))
    set_run_font(run, size=10)
    run.font.name = "Consolas"
    run._element.rPr.rFonts.set(qn("w:eastAsia"), "Consolas")
    p.paragraph_format.space_before = Pt(6)
    p.paragraph_format.space_after = Pt(10)
    p.paragraph_format.left_indent = Inches(0.15)


def build():
    doc = Document()

    section = doc.sections[0]
    section.top_margin = Inches(0.85)
    section.bottom_margin = Inches(0.85)
    section.left_margin = Inches(0.9)
    section.right_margin = Inches(0.9)

    title = doc.add_paragraph()
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = title.add_run("MedReach")
    set_run_font(r, size=28, bold=True, color=RGBColor(0x00, 0x7F, 0x7F))

    sub = doc.add_paragraph()
    sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = sub.add_run("Operational Flows Guide")
    set_run_font(r, size=18, bold=True)

    meta = doc.add_paragraph()
    meta.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = meta.add_run(
        "Clinical operations, patient journey, outreach programs, Field PWA, and administration\n"
        "Hospital Web · Field tablets · API\n"
        "Production: https://medreach.ecews.org  |  Field: https://medreach.ecews.org/field"
    )
    set_run_font(r, size=10)
    r.font.color.rgb = RGBColor(0x55, 0x55, 0x55)

    add_para(
        doc,
        "This guide explains how MedReach works for different hospital and outreach operations. "
        "Use it to train staff on the correct sequence of steps in the Web app and on Field tablets.",
        italic=True,
    )

    # ---- 1 ----
    add_heading(doc, "1. System overview", 1)
    add_table(
        doc,
        ["Surface", "Purpose", "Typical access"],
        [
            ["Hospital Web", "Full clinical work, admin, stakeholders", "https://medreach.ecews.org"],
            ["Field PWA", "Offline outreach on Android tablets", "https://medreach.ecews.org/field"],
            ["API", "Data, referrals, Field sync", "https://medreach.ecews.org/api"],
        ],
    )
    add_para(doc, "Web sidebar areas: Home · Patients · Modules · Outreach · Stakeholders · Admin · Inbox · Sign out.")

    # ---- 2 ----
    add_heading(doc, "2. Login, roles, and module access", 1)
    add_heading(doc, "2.1 Sign-in", 2)
    add_bullets(
        doc,
        [
            "Open MedReach. Unauthenticated users go to /login.",
            "Enter Username and Password → Sign in.",
            "On success you land on Home (/). The sidebar shows only modules allowed for your role.",
            "Opening a module without permission shows: “You do not have access to this module.”",
            "Sign out from the user menu returns you to /login.",
        ],
        numbered=True,
    )
    add_para(doc, "Demo / seed password pattern: Role@123 (example: admin / Admin@123).")

    add_heading(doc, "2.2 Seeded roles", 2)
    add_table(
        doc,
        ["Username", "Role", "Typical modules"],
        [
            ["admin", "Administrator", "All modules + Admin"],
            ["nurse", "Nurse", "Patients, Triage, Patient Dashboard"],
            ["clinician", "Clinician", "Patients, Triage, Consultations, Pharmacy, Laboratory, Dashboard"],
            ["pharmacist", "Pharmacist", "Patients, Pharmacy, Dashboard"],
            ["lab", "Lab Technician", "Patients, Laboratory, Dashboard"],
            ["dental", "Dental Officer", "Patients, Dental, Dashboard"],
            ["eyecare", "Eye Care", "Patients, Optometrists, Ophthalmologists, Dashboard"],
            ["stakeholder", "Stakeholder", "Stakeholders only"],
        ],
    )

    add_heading(doc, "2.3 Module catalog", 2)
    add_table(
        doc,
        ["Module (UI)", "Route", "Notes"],
        [
            ["Patients", "/patients", "Registration and list"],
            ["Triage", "/triage", "Vitals and history"],
            ["Consultations", "/consultations", "Diagnosis and treatment"],
            ["Pharmacy", "/pharmacy", "Prescribe and dispense"],
            ["Laboratory", "/laboratory", "Tests and results"],
            ["Dental", "/dental", "Dental visits"],
            ["Ancillary", "/ancillary", "Ancillary / antenatal-style services"],
            ["Optometrists", "/optometrists", "Optometry / Eye Clinic"],
            ["Ophthalmologists", "/ophthalmologists", "Ophthalmology"],
            ["Patient Dashboard", "/patient-dashboard/{id}", "Per-patient hub"],
            ["Stakeholders", "/stakeholders", "KPIs and charts"],
            ["Secondary Outreach", "/secondary-outreach", "Deworming, ITN, clinic lists"],
            ["Administration", "/admin/...", "Users, roles, settings, programs"],
        ],
    )

    # ---- 3 ----
    add_heading(doc, "3. Patient registration and lifecycle", 1)
    add_heading(doc, "3.1 Patients list — /patients", 2)
    add_bullets(
        doc,
        [
            "Search by patient number, phone, name, or outreach program.",
            "Edit /patients/edit/{id} or view /patients/details/{id}.",
            "Facility mode: New Patient → /patients/create.",
            "Outreach mode (active primary care program): Register (Outreach) → /patients/register-outreach.",
        ],
    )

    add_heading(doc, "3.2 Standard registration — /patients/create", 2)
    add_bullets(
        doc,
        [
            "Preview next patient number (prefix from Settings; assigned on save).",
            "Identity: Full name*, Sex*, Age, Age unit (Years/Months).",
            "Background: Marital status, Tribe, Religion, Occupation.",
            "Contact: Phone, Address.",
            "Create Patient → return to /patients → continue via Patient Dashboard or a clinical module.",
        ],
        numbered=True,
    )

    add_heading(doc, "3.3 Outreach registration — /patients/register-outreach", 2)
    add_bullets(
        doc,
        [
            "Requires Outreach mode: primary Care Program Active + outreach enabled in Settings.",
            "ID mode Auto serial (assigned on save) or Pre-printed IDs (Verify ID must be Available).",
            "Capture demographics and save → patient linked to the active primary program.",
            "Checklist in UI: Registration → Triage → Laboratory → Consultation → Pharmacy.",
        ],
        numbered=True,
    )

    add_heading(doc, "3.4 Patient Dashboard — /patient-dashboard/{id}", 2)
    add_bullets(
        doc,
        [
            "Shows profile, patient journey (referral history), and clinical history.",
            "Alerts when Pharmacy is pending → link to dispense.",
            "Start encounter chips (permission-gated): Triage, Consultation, Prescribe, Dispense, Laboratory, Dental, Ancillary, Optometrist, Ophthalmologist.",
        ],
    )

    add_heading(doc, "3.5 Lifecycle summary", 2)
    add_flow_box(
        doc,
        [
            "Register (standard or outreach)",
            "  → Patient record + ClientNumber",
            "  → Triage / encounters from dashboard or module queue",
            "  → Cross-module referrals (Pending → InProgress → Completed)",
            "  → Pharmacy PENDING / DISPENSED / DECLINED",
            "  → Optional secondary outreach enrollment / clinic referral",
            "  → Visible on Stakeholders analytics",
        ],
    )

    # ---- 4 ----
    add_heading(doc, "4. Referral and queue model (shared by clinical modules)", 1)
    add_para(
        doc,
        "Most clinical modules (except Triage as a destination) show a Referral queue at the top of the index. "
        "The sidebar Inbox bell also shows personal referral notifications.",
    )
    add_table(
        doc,
        ["Status", "Meaning", "Typical action"],
        [
            ["Pending", "Waiting in destination queue", "Attend"],
            ["InProgress", "Claimed / being seen", "Continue, Return, or Done"],
            ["Completed", "Finished at destination", "—"],
            ["Cancelled", "Cancelled or superseded", "—"],
        ],
    )
    add_para(doc, "Priorities when referring: Emergency (~5 min) · Urgent (~15 min) · Routine (~30 min).")
    add_heading(doc, "4.1 Queue actions", 2)
    add_bullets(
        doc,
        [
            "Attend — claim Pending → InProgress and open the create/dispense screen for that patient.",
            "Continue — resume an InProgress encounter.",
            "Return — send back to Pending.",
            "Done — mark Completed without creating a new clinical record.",
            "Reassign — move the referral to another clinical module.",
            "Saving a clinical record for a patient automatically completes active referrals targeting that module.",
        ],
    )
    add_para(
        doc,
        "Manual referral: after save on many create screens, use “Refer to another module” "
        "(priority, destinations, notes) or “Done without referring.”",
    )

    # ---- 5 ----
    add_heading(doc, "5. Clinical operations — module flows", 1)

    add_heading(doc, "5.1 Triage — /triage", 2)
    add_bullets(
        doc,
        [
            "Find patient → Begin triage → /triage/create/{patientId}.",
            "Record vitals/history (BP, temp, weight/height, BMI, chronic flags, etc.).",
            "Save triage.",
            "Facility mode: auto-refer to Consultations (Pending).",
            "Outreach mode: auto-refer to Laboratory (Pending).",
            "Outreach with Dental/Eye secondary programs: optional modal to refer to Dental Clinic and/or Eye Clinic.",
        ],
        numbered=True,
    )

    add_heading(doc, "5.2 Laboratory — /laboratory", 2)
    add_bullets(
        doc,
        [
            "Open Laboratory → Referral queue (from Triage or other modules).",
            "Attend queue or search patient → /laboratory/create/{patientId}.",
            "Enter tests and results; save; optional further referral.",
            "Outreach mode: auto-refer next to Consultations.",
            "Result may show as pending until entered.",
        ],
        numbered=True,
    )

    add_heading(doc, "5.3 Consultations — /consultations", 2)
    add_bullets(
        doc,
        [
            "Open Consultations → Referral queue → Attend or create /consultations/create/{patientId}.",
            "Capture diagnoses, treatments, services referred; optional ITN Order / ITN Dispense.",
            "Save to dashboard, or Save & refer via referral modal.",
            "Outreach mode: auto-refer to Pharmacy after save.",
        ],
        numbered=True,
    )

    add_heading(doc, "5.4 Pharmacy — /pharmacy", 2)
    add_bullets(
        doc,
        [
            "Open Pharmacy → Referral queue + prescription list.",
            "Prescribe: /pharmacy/create/{patientId} → drug, dosage, frequency, duration → status PENDING.",
            "Dispense: /pharmacy/dispense/{patientId} → set each Rx to PENDING, DISPENSED, or DECLINED.",
            "Saving dispensation completes Pharmacy-targeted referrals when appropriate.",
            "Patient Dashboard shows Pharmacy pending when applicable.",
        ],
        numbered=True,
    )

    add_heading(doc, "5.5 Dental — /dental", 2)
    add_bullets(
        doc,
        [
            "Queue + list on /dental → create /dental/create/{patientId}.",
            "Diagnoses, treatments, dispensed items, services referred, referred flag.",
            "Optional post-save referral. Completing the visit completes Dental-targeted referrals.",
            "In outreach, patients often arrive after Triage refers them to Dental Clinic.",
        ],
    )

    add_heading(doc, "5.6 Optometrists & Ophthalmologists", 2)
    add_bullets(
        doc,
        [
            "Optometrists /optometrists — exam, glasses dispensed, referred; Eye Clinic secondary links here.",
            "Ophthalmologists /ophthalmologists — ophthalmology exam; same queue pattern.",
            "Save completes referrals targeting that module; optional further referral modal.",
        ],
    )

    add_heading(doc, "5.7 Ancillary — /ancillary", 2)
    add_bullets(
        doc,
        [
            "Queue + list → create /ancillary/create/{patientId}.",
            "Services catalog and pregnancy status (antenatal / ancillary capture).",
            "Same referral queue pattern as other clinical modules.",
        ],
    )

    add_heading(doc, "5.8 Facility vs outreach clinical paths", 2)
    add_para(doc, "Facility mode (no active primary outreach):", bold=True)
    add_flow_box(
        doc,
        [
            "Patients → Triage → (auto) Consultations [Pending]",
            "                 ↘ optional refer Lab / Pharmacy / Dental / Eye / Ancillary",
            "Consultations → optional refer → Pharmacy / Lab / others",
            "Pharmacy → Prescribe (PENDING) → Dispense (DISPENSED / DECLINED)",
        ],
    )
    add_para(doc, "Outreach mode (active primary program):", bold=True)
    add_flow_box(
        doc,
        [
            "Register (Outreach) → Triage",
            "  → (auto) Laboratory",
            "  → (auto) Consultations",
            "  → (auto) Pharmacy",
            "  + optional Triage referral → Dental Clinic / Eye Clinic queues",
        ],
    )

    # ---- 6 ----
    add_heading(doc, "6. Outreach programs (primary & secondary)", 1)
    add_heading(doc, "6.1 Concepts", 2)
    add_table(
        doc,
        ["Type", "Where configured", "Behavior"],
        [
            ["Primary", "Admin → Care Programs", "Full registration + clinical flow; enables Outreach mode"],
            ["Secondary", "Care Programs + Outreach nav", "Runs alongside primary (Deworming, ITN, clinics)"],
        ],
    )
    add_table(
        doc,
        ["Secondary program (UI)", "Sidebar label", "Enrollment style"],
        [
            ["Patient Deworming", "Deworming", "Registration-only"],
            ["Insecticide Treated Net (ITN)", "ITN", "Registration-only"],
            ["Dental Clinic", "Dental", "Clinic-linked → Dental module queue"],
            ["Eye Clinic", "Eye Clinic", "Clinic-linked → Optometrists queue"],
        ],
    )
    add_para(doc, "Care program statuses: Draft · Active · Ended. Patient ID modes: AutoSerial · PreGenerated (Available → Registered → Voided).")

    add_heading(doc, "6.2 Admin setup — /admin/care-programs", 2)
    add_bullets(
        doc,
        [
            "Create program or Quick create (Deworming / ITN / Dental / Eye).",
            "Configure name, dates, community, ID mode, batch size, outreach code; assign staff.",
            "Optionally Generate IDs for pre-printed cards.",
            "Activate primary → Patients CTA becomes Register (Outreach).",
            "Activate secondary programs for concurrent campaigns.",
            "End when the campaign finishes.",
        ],
        numbered=True,
    )

    add_heading(doc, "6.3 Deworming / ITN (registration-only)", 2)
    add_bullets(
        doc,
        [
            "Outreach → Deworming or ITN → Register (/secondary-outreach/register/{programId}).",
            "Enter Name*, Age, Gender*, Status*.",
            "Deworming statuses: Dewormed, Pending, Referred, Not Eligible.",
            "ITN statuses: Net Issued, Pending, Referred, Not Eligible.",
            "Optional Card ID if PreGenerated. Save → appears on enrolled list with filters.",
        ],
        numbered=True,
    )

    add_heading(doc, "6.4 Dental / Eye clinics (clinic-linked)", 2)
    add_bullets(
        doc,
        [
            "Do not register directly on the secondary register form for clinics.",
            "Primary patient → Triage → after save, refer to Dental Clinic and/or Eye Clinic.",
            "Patient appears in Dental (/dental) or Optometrists (/optometrists) Referral queue as Pending.",
            "Enrolled list shows clinic enrollments with referral status: Pending, InProgress, Completed.",
            "Clinic queue buttons jump to the clinical module.",
        ],
        numbered=True,
    )

    # ---- 7 ----
    add_heading(doc, "7. Stakeholders dashboard — /stakeholders", 1)
    add_bullets(
        doc,
        [
            "Facility overview tab: total patients, encounters, MoM growth, pharmacy dispense rate, pending Rx, surveillance (Malaria/HIV/TB), demographics and trends.",
            "Per-outreach program tabs: registrations, targets, status mix, gender/age, registration trend.",
            "Read-only analytics — no clinical data entry.",
        ],
    )

    # ---- 8 ----
    add_heading(doc, "8. Administration", 1)
    add_table(
        doc,
        ["Area", "Route", "What you do"],
        [
            ["Users", "/admin/users", "Create/edit users, role, active flag, module overrides"],
            ["Roles & modules", "/admin/roles", "Tick modules each role may open"],
            ["Care programs", "/admin/care-programs", "Primary/secondary outreach campaigns"],
            ["Settings", "/admin/settings", "Facility name, ID prefix, timezone, enable modules/outreach"],
            ["LAN access", "/admin/lan-access", "Hospital LAN / staff URL configuration"],
            ["Backup", "/admin/backup", "Create, download, restore backups"],
            ["Export", "/admin/export", "Download spreadsheets by module / date / outreach"],
            ["Field devices", "/admin/field-devices", "Tablets that signed into Field / synced"],
        ],
    )

    # ---- 9 ----
    add_heading(doc, "9. Field PWA offline flow", 1)
    add_para(
        doc,
        "Install on Android Chrome: open https://medreach.ecews.org/field → Install app / Add to Home Screen. "
        "Nav: Home · Register · Patients · Secondary · Clinical · Sync · Device.",
    )
    add_heading(doc, "9.1 First-time bootstrap (online)", 2)
    add_bullets(
        doc,
        [
            "Sign in with a hospital account while Online.",
            "Set Device label (e.g. Tablet-North-Camp-01).",
            "Refresh settings → caches facility, active programs, and clinical catalogs (IndexedDB).",
            "JWT refresh grace (~14 days) supports intermittent connectivity.",
        ],
        numbered=True,
    )
    add_heading(doc, "9.2 Offline capture", 2)
    add_bullets(
        doc,
        [
            "Register patient, work local patients list, capture Triage / Lab / Consultation / Pharmacy / Dental / Eye / Secondary.",
            "Each save enqueues a pending record locally (no immediate API required).",
        ],
    )
    add_heading(doc, "9.3 Sync — /sync", 2)
    add_bullets(
        doc,
        [
            "When signal returns: background sync and/or Push to hospital / Sync now.",
            "Queue shows When, Type, Name, Status (pending / synced / failed).",
            "Conflicts: Retry, Discard, Details (missing patient, invalid ID, inactive program, permission).",
            "Successful push creates hospital records and advances referral/outreach state.",
        ],
    )
    add_table(
        doc,
        ["Field route", "Hospital equivalent"],
        [
            ["/register", "Outreach registration"],
            ["/patients", "Local subset of Patients"],
            ["/secondary", "Secondary outreach register"],
            ["/triage … /pharmacy", "Same clinical modules"],
            ["/dental", "Dental"],
            ["/eye", "Optometrists / Eye Clinic"],
            ["/sync", "api/field-sync"],
            ["/device", "Admin → Field devices"],
        ],
    )

    # ---- 10 ----
    add_heading(doc, "10. End-to-end journeys", 1)

    add_heading(doc, "10.1 Standard hospital outpatient (facility mode)", 2)
    add_bullets(
        doc,
        [
            "Reception: /patients/create → New Patient.",
            "Open Patient Dashboard → Triage → save → Consultations queue Pending.",
            "Clinician Attend → consultation → optional refer Lab / Pharmacy / Dental / Eye.",
            "Lab Attend → results → Complete.",
            "Pharmacy Prescribe (PENDING) → Dispense (DISPENSED / DECLINED).",
            "Journey on dashboard; KPIs on Stakeholders.",
        ],
        numbered=True,
    )

    add_heading(doc, "10.2 Community outreach day (primary + concurrent secondary)", 2)
    add_bullets(
        doc,
        [
            "Admin activates Primary + Deworming/ITN + Dental/Eye secondaries.",
            "Registration desk: /patients/register-outreach (or Field /register).",
            "Triage → auto-queue Laboratory; optionally refer Dental and/or Eye Clinic.",
            "Lab → auto-queue Consultations → auto-queue Pharmacy; ITN order if needed.",
            "Pharmacy dispenses meds/ITN.",
            "Parallel: Deworming/ITN teams register on secondary outreach with status Dewormed / Net Issued.",
            "Dental/Eye teams work their Referral queues.",
            "Field tablets sync when connectivity returns; Stakeholders review Facility + program tabs.",
        ],
        numbered=True,
    )

    add_heading(doc, "10.3 Pharmacy-only follow-up", 2)
    add_bullets(
        doc,
        [
            "Patient already registered → /pharmacy/create/{id} → prescribe.",
            "Dashboard shows Pharmacy pending → /pharmacy/dispense/{id} → DISPENSED or DECLINED.",
        ],
        numbered=True,
    )

    add_heading(doc, "10.4 Specialty visit (Dental / Eye, non-outreach)", 2)
    add_bullets(
        doc,
        [
            "From Patient Dashboard or module search: create Optometrist / Ophthalmologist / Dental record.",
            "Or clinician refers from Consultation → specialty Pending queue → Attend → complete.",
        ],
        numbered=True,
    )

    # ---- 11 ----
    add_heading(doc, "11. Status cheat sheet", 1)
    add_table(
        doc,
        ["Domain", "Statuses"],
        [
            ["Referral", "Pending → InProgress → Completed (or Cancelled)"],
            ["Pharmacy", "PENDING · DISPENSED · DECLINED"],
            ["Care program", "Draft · Active · Ended"],
            ["Program patient ID", "Available · Registered · Voided"],
            ["Secondary (Deworm)", "Dewormed · Pending · Referred · Not Eligible"],
            ["Secondary (ITN)", "Net Issued · Pending · Referred · Not Eligible"],
            ["Clinic enrollment list", "Pending · InProgress · Completed (from referral)"],
            ["Field sync queue", "pending / synced / failed"],
            ["Lab result UI", "May show pending until result entered"],
        ],
    )

    # ---- 12 ----
    add_heading(doc, "12. Route index", 1)
    add_heading(doc, "12.1 Hospital Web", 2)
    add_table(
        doc,
        ["Area", "Routes"],
        [
            ["Auth / Home", "/login · /"],
            ["Patients", "/patients · /create · /edit/{id} · /details/{id} · /register-outreach"],
            ["Dashboard", "/patient-dashboard/{id}"],
            [
                "Clinical",
                "/triage · /consultations · /pharmacy · /laboratory · /dental · /ancillary · "
                "/optometrists · /ophthalmologists (+ /create/{patientId}, /edit/{id}, pharmacy /dispense/{patientId})",
            ],
            ["Outreach", "/secondary-outreach · /register/{id} · /{id}/enrolled"],
            ["Stakeholders", "/stakeholders"],
            [
                "Admin",
                "/admin/users · /roles · /care-programs · /settings · /lan-access · /backup · /export · /field-devices",
            ],
        ],
    )
    add_heading(doc, "12.2 Field PWA", 2)
    add_para(
        doc,
        "/ · /login · /register · /patients · /secondary · /triage · /laboratory · /consultation · "
        "/pharmacy · /dental · /eye · /sync · /device",
    )

    footer = doc.add_paragraph()
    footer.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = footer.add_run(
        "\n— End of MedReach Operational Flows Guide —\n"
        "Generated for hospital training and operations reference."
    )
    set_run_font(r, size=9)
    r.font.color.rgb = RGBColor(0x77, 0x77, 0x77)

    OUT.parent.mkdir(parents=True, exist_ok=True)
    doc.save(OUT)
    print(str(OUT))


if __name__ == "__main__":
    build()
