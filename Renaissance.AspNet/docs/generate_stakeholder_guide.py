from docx import Document
from docx.shared import Pt, RGBColor, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
from datetime import date
import os

out = r"C:\Users\ADMIN\Downloads\renaissance-plugin\Renaissance.AspNet\docs\MedReach-Stakeholder-Presentation-Guide.docx"
os.makedirs(os.path.dirname(out), exist_ok=True)

doc = Document()
for section in doc.sections:
    section.top_margin = Cm(2.0)
    section.bottom_margin = Cm(2.0)
    section.left_margin = Cm(2.2)
    section.right_margin = Cm(2.2)

normal = doc.styles["Normal"]
normal.font.name = "Calibri"
normal.font.size = Pt(11)
normal.font.color.rgb = RGBColor(0x1F, 0x2A, 0x28)
normal.paragraph_format.space_after = Pt(8)
normal.paragraph_format.line_spacing = 1.15

TEAL = RGBColor(0x0D, 0x6E, 0x6E)
DARK = RGBColor(0x1F, 0x2A, 0x28)
MUTED = RGBColor(0x5F, 0x6E, 0x6A)
ACCENT = RGBColor(0xD4, 0xA5, 0x74)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)


def set_run_font(run, size=11, bold=False, color=None, name="Calibri"):
    run.font.name = name
    run._element.rPr.rFonts.set(qn("w:eastAsia"), name)
    run.font.size = Pt(size)
    run.bold = bold
    if color:
        run.font.color.rgb = color


def shade_cell(cell, hex_color):
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:fill"), hex_color)
    shd.set(qn("w:val"), "clear")
    tcPr.append(shd)


def set_cell_border(cell):
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    tcBorders = OxmlElement("w:tcBorders")
    for edge in ("top", "left", "bottom", "right"):
        el = OxmlElement(f"w:{edge}")
        el.set(qn("w:val"), "single")
        el.set(qn("w:sz"), "4")
        el.set(qn("w:color"), "D0D7D5")
        tcBorders.append(el)
    tcPr.append(tcBorders)


def add_heading_custom(text, level=1):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(16 if level == 1 else 12)
    p.paragraph_format.space_after = Pt(6)
    run = p.add_run(text)
    if level == 1:
        set_run_font(run, 18, True, TEAL)
    elif level == 2:
        set_run_font(run, 14, True, TEAL)
    else:
        set_run_font(run, 12, True, DARK)
    return p


def add_body(text):
    p = doc.add_paragraph()
    run = p.add_run(text)
    set_run_font(run, 11, False, DARK)
    return p


def add_bullet(text):
    p = doc.add_paragraph(style="List Bullet")
    p.clear()
    p.paragraph_format.left_indent = Cm(0.5)
    p.paragraph_format.space_after = Pt(4)
    run = p.add_run(text)
    set_run_font(run, 11, False, DARK)
    return p


def add_table(headers, rows):
    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    hdr = table.rows[0].cells
    for i, h in enumerate(headers):
        hdr[i].text = ""
        p = hdr[i].paragraphs[0]
        run = p.add_run(h)
        set_run_font(run, 10, True, WHITE)
        shade_cell(hdr[i], "0D6E6E")
        set_cell_border(hdr[i])
    for r_i, row in enumerate(rows):
        cells = table.rows[r_i + 1].cells
        for c_i, val in enumerate(row):
            cells[c_i].text = ""
            p = cells[c_i].paragraphs[0]
            run = p.add_run(str(val))
            set_run_font(run, 10, False, DARK)
            if r_i % 2 == 1:
                shade_cell(cells[c_i], "F4F8F7")
            set_cell_border(cells[c_i])
    doc.add_paragraph()
    return table


def add_callout(title, body):
    table = doc.add_table(rows=1, cols=1)
    cell = table.rows[0].cells[0]
    shade_cell(cell, "E6F4F4")
    set_cell_border(cell)
    cell.text = ""
    p1 = cell.paragraphs[0]
    r1 = p1.add_run(title)
    set_run_font(r1, 11, True, TEAL)
    p2 = cell.add_paragraph()
    r2 = p2.add_run(body)
    set_run_font(r2, 10, False, DARK)
    doc.add_paragraph()


# Cover
for _ in range(2):
    doc.add_paragraph()

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(p.add_run("MedReach"), 36, True, TEAL)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(
    p.add_run("Clinical Data Collection & Community Outreach Platform"),
    16,
    False,
    DARK,
)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(p.add_run("Stakeholder Presentation Guide"), 14, True, ACCENT)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(
    p.add_run("How MedReach supports facilities, outreach teams, and decision-makers"),
    11,
    False,
    MUTED,
)

doc.add_paragraph()
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(
    p.add_run(f"Prepared for stakeholder briefing  ·  {date.today().strftime('%B %Y')}"),
    10,
    False,
    MUTED,
)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(p.add_run("Production: https://medreach.ecews.org"), 10, False, TEAL)

doc.add_page_break()

# 1
add_heading_custom("1. Why MedReach", 1)
add_body(
    "MedReach is a purpose-built clinical data platform for hospitals and community medical outreach. "
    "It connects patient registration, clinical stations, specialty clinics, pharmacy, laboratory, and "
    "campaign-style programs (such as deworming and ITN distribution) into one coordinated workflow — "
    "with a Field tablet app that continues working when internet is unreliable."
)
add_callout(
    "Adoption value for stakeholders",
    "One patient record across every station. Clear referral queues so nobody is lost between Triage, Lab, "
    "Consultation, and Pharmacy. Outreach campaigns with trackable registrations. Live dashboards for disease "
    "surveillance and program performance. Offline Field tablets for communities beyond reliable connectivity.",
)
add_heading_custom("What stakeholders get", 2)
add_bullet("Visibility: facility-wide and per-outreach KPIs, including malaria / HIV / TB surveillance signals.")
add_bullet("Accountability: every encounter is linked to a patient, station, and referral journey.")
add_bullet("Coverage: hospital web for facility workstations + Field PWA for community teams.")
add_bullet("Control: role-based access so clinicians, lab, pharmacy, and stakeholders only see what they need.")

# 2
add_heading_custom("2. Platform at a glance", 1)
add_table(
    ["Component", "Who uses it", "Access"],
    [
        ["Hospital Web App", "Facility clinical & admin staff", "https://medreach.ecews.org"],
        ["API / Backend", "Secure data services + SQL Server", "https://medreach.ecews.org/api"],
        ["Field PWA (tablets)", "Outreach teams in communities", "https://medreach.ecews.org/field"],
    ],
)
add_body(
    "Staff sign in with hospital accounts. The Field app uses the same credentials: sign in once while online, "
    "then continue capturing visits offline and sync when connectivity returns."
)

# 3
add_heading_custom("3. The patient journey", 1)
add_body(
    "MedReach is designed around the real movement of a patient through an outreach or outpatient day. "
    "Each station hands the patient forward through a referral queue, and the full path is visible on the patient dashboard."
)

add_heading_custom("3.1 Primary outreach clinical flow", 2)
add_body("When a Primary care program is active (outreach mode), the standard journey is:")

steps = [
    (
        "1. Registration",
        "Patient is registered into the active Primary outreach with a unique outreach ID (auto-serial or pre-printed batch).",
    ),
    (
        "2. Triage",
        "Vitals and presenting history are recorded. The system automatically refers the patient to Laboratory (and optionally Dental or Eye clinic).",
    ),
    (
        "3. Laboratory",
        "Tests are ordered and results captured (including surveillance-relevant tests such as malaria, HIV, TB).",
    ),
    (
        "4. Consultation",
        "Clinician records diagnosis and treatment plan; can flag ITN order/dispense as needed.",
    ),
    (
        "5. Pharmacy",
        "Medications are prescribed and dispensed; pending orders are visible in queue.",
    ),
]
for title, desc in steps:
    p = doc.add_paragraph()
    set_run_font(p.add_run(title + " — "), 11, True, TEAL)
    set_run_font(p.add_run(desc), 11, False, DARK)

add_heading_custom("3.2 Specialty clinic path", 2)
add_body(
    "From Triage, patients can also be referred to Dental and/or Eye clinic. Eye Clinic outreach is linked to the "
    "Optometrists module; Ophthalmology remains available for advanced eye care."
)

add_heading_custom("3.3 Registration-only secondary outreach", 2)
add_body(
    "Some campaigns do not require a full clinical path. Deworming and Insecticide Treated Net (ITN) programs use a "
    "streamlined Secondary Outreach registration flow — fast community capture with full stakeholder reporting."
)

add_heading_custom("3.4 Visit journey (patient dashboard)", 2)
add_bullet("Open any patient → Patient Dashboard.")
add_bullet("“Visit journey” shows each station step: Pending, In Progress, or Completed.")
add_bullet("Priority (Routine / Urgent / Emergency), referred-by, and attended-by are visible.")
add_bullet(
    "Clinical history across Triage, Consultation, Pharmacy, Lab, Dental, Ancillary, Optometry, and Ophthalmology."
)
add_callout(
    "Why this matters",
    "Stakeholders can trust that community patients are not “lost between stations.” Supervisors see queues and "
    "journey status; program managers see who was reached and what care was delivered.",
)

# 4
add_heading_custom("4. Setting up outreaches (Care Programs)", 1)
add_body(
    "Outreaches are configured under Admin → Care Programs. There are two program types that work together."
)
add_table(
    ["Type", "Purpose", "Example"],
    [
        [
            "Primary",
            "Full clinical outreach mode. Activating a Primary program switches the facility into outreach workflow (registration + Lab → Consult → Pharmacy path).",
            "Community Health Outreach",
        ],
        [
            "Secondary",
            "Runs alongside Primary. Either registration-only campaigns or clinic-linked specialty outreaches.",
            "Deworming, ITN, Dental Clinic, Eye Clinic",
        ],
    ],
)

add_heading_custom("4.1 Recommended setup sequence", 2)
for i, t in enumerate(
    [
        "Sign in as Administrator.",
        "Go to Admin → Care Programs.",
        "Create or open a Primary program (name, outreach code, start/end dates, patient ID mode).",
        "Activate the Primary program to enter outreach mode.",
        "Add Secondary programs as needed (quick templates available for Deworming, ITN, Dental, Eye).",
        "For clinic-linked Secondary programs, confirm the linked clinical module (Dental or Optometrists).",
        "Optionally generate patient ID batches for pre-printed cards.",
        "Assign Field devices under Admin → Field Devices and brief tablet teams.",
    ],
    1,
):
    add_bullet(f"{i}. {t}")

add_heading_custom("4.2 Outreach programs stakeholders will recognize", 2)
add_table(
    ["Program", "Code", "Kind", "What it captures"],
    [
        [
            "Community Health Outreach (Primary)",
            "CHO (example)",
            "Full clinical flow",
            "Registration through Triage → Lab → Consult → Pharmacy",
        ],
        [
            "Patient Deworming",
            "DEWORM",
            "Secondary · registration",
            "Community deworming registrations",
        ],
        [
            "Insecticide Treated Net (ITN)",
            "ITN",
            "Secondary · registration",
            "Net distribution registrations",
        ],
        [
            "Dental Clinic",
            "DENTAL",
            "Secondary · clinic-linked",
            "Dental consultations & treatments",
        ],
        [
            "Eye Clinic",
            "EYE",
            "Secondary · clinic-linked",
            "Optometry exams (visual acuity, glasses)",
        ],
    ],
)

add_heading_custom("4.3 Patient identification", 2)
add_bullet("Auto serial: system issues sequential outreach IDs as patients are registered.")
add_bullet("Pre-generated batches: print cards in advance (e.g. CHO-0001…) and assign during registration.")
add_bullet("Facility branding and client-number prefix are configurable in Admin → Settings.")

# 5
add_heading_custom("5. Clinical & support modules", 1)
add_body("Each module is a station with its own queue, forms, and history — coordinated by referrals.")
add_table(
    ["Module", "What staff do"],
    [
        ["Patients", "Register and manage patient identity, demographics, and outreach linkage"],
        ["Triage", "Record vitals and history; start the referral chain"],
        ["Laboratory", "Order/record tests and results for clinical care and surveillance"],
        ["Consultations", "Diagnose, treat, and continue referrals (including ITN flags)"],
        ["Pharmacy", "Prescribe and dispense medications; clear pending orders"],
        ["Dental", "Dental examination, diagnoses, and treatments"],
        ["Optometrists", "Visual acuity and glasses dispensing for eye outreach"],
        ["Ophthalmologists", "Advanced eye diagnosis, treatment, and surgery documentation"],
        ["Ancillary", "Support services such as family planning"],
        ["Secondary Outreach", "Fast registration for Deworming / ITN-style campaigns"],
        ["Patient Dashboard", "One-patient view: journey, history, and next actions"],
        ["Stakeholders", "Executive KPIs, disease surveillance, and outreach performance"],
        [
            "Administration",
            "Users, roles, settings, care programs, backup, export, Field devices",
        ],
    ],
)

# 6
add_heading_custom("6. Referrals & station queues", 1)
add_bullet("Every clinical station shows a referral queue (priority first, then arrival order).")
add_bullet("Staff claim a patient, complete the encounter, and the journey advances.")
add_bullet("Live notifications alert teams when new referrals arrive.")
add_bullet("SLA-style wait indicators help supervisors spot bottlenecks.")
add_body(
    "This is the operational heart of MedReach: it turns a busy outreach day into an orderly flow instead of paper slips and verbal hand-offs."
)

# 7
add_heading_custom("7. Stakeholders dashboard", 1)
add_body("Role: Stakeholders (and Administrators). Route: /stakeholders")
add_heading_custom("Facility overview", 2)
add_bullet("Total patients and new registrations this month")
add_bullet("Total encounters and month-over-month growth")
add_bullet("Pharmacy dispense rate and pending orders")
add_bullet("Average encounters per patient and projected monthly volume")
add_bullet("Priority disease surveillance: malaria, HIV, TB positives, positivity rates, recent alerts")
add_bullet("Antenatal laboratory surveillance for pregnant patients")
add_bullet("Charts: encounter volume & forecast, module share, demographics, pharmacy status")
add_heading_custom("Per-outreach tabs", 2)
add_bullet("Registrations (total and this month)")
add_bullet("Target batch progress where applicable")
add_bullet("Status, gender, and age breakdowns for each active program")
add_callout(
    "Decision support",
    "Program sponsors and ministry / NGO stakeholders can monitor reach and quality without opening clinical charts. "
    "Surveillance signals surface early for malaria, HIV, and TB findings recorded in Laboratory.",
)

# 8
add_heading_custom("8. Field tablets (offline outreach)", 1)
add_body(
    "MedReach Field is a Progressive Web App designed for Android tablets in communities with intermittent internet."
)
add_bullet("Sign in once online — caches outreach settings and clinical catalogs")
add_bullet(
    "Register patients and capture Triage, Lab, Consultation, Pharmacy, Dental, Eye, and Secondary visits offline"
)
add_bullet("Queue syncs to the hospital when connectivity returns (manual or background sync)")
add_bullet("Conflict handling for contested records (retry / discard)")
add_bullet("Device labeling so Admin can see which tablets are active")
add_bullet("Install to home screen for an app-like experience (HTTPS in production)")
add_body("Field URL: https://medreach.ecews.org/field")

# 9
add_heading_custom("9. Security, roles & governance", 1)
add_bullet("Authentication required for web and Field")
add_bullet(
    "Role-based module access (Administrator, Nurse, Clinician, Pharmacist, Lab, Dental, Eye Care, Stakeholder, and custom roles)"
)
add_bullet("API enforces the same permissions as the UI")
add_bullet("Passwords stored with modern hashing; sessions use secure tokens")
add_bullet("Field refresh grace supports multi-day campaigns without daily reconnect")
add_bullet("Admin tools for backup and clinical export")

# 10
add_heading_custom("10. Suggested live demo script (20–25 minutes)", 1)
demo = [
    (
        "Minutes 0–3 · Opening",
        "Show the login and home screen. State the problem: fragmented paper outreach data and lost follow-up between stations.",
    ),
    (
        "Minutes 3–7 · Outreach setup",
        "Admin → Care Programs. Show Primary + Secondary programs (Deworming, ITN, Dental, Eye). Explain Activate = outreach mode.",
    ),
    (
        "Minutes 7–12 · Patient journey",
        "Register a patient → Triage → show auto-referral to Laboratory → open Patient Dashboard Visit journey.",
    ),
    (
        "Minutes 12–16 · Station queues",
        "Open Laboratory / Consultation / Pharmacy queues. Claim and complete one step so stakeholders see hand-off.",
    ),
    (
        "Minutes 16–19 · Secondary campaigns",
        "Secondary Outreach for Deworming or ITN — fast registration for high-volume community days.",
    ),
    (
        "Minutes 19–22 · Stakeholders dashboard",
        "KPIs, disease surveillance, and per-outreach tabs. Emphasize decision-ready views.",
    ),
    (
        "Minutes 22–25 · Field offline",
        "Open /field on a tablet or phone. Explain offline capture + sync. Close with adoption ask.",
    ),
]
for title, desc in demo:
    p = doc.add_paragraph()
    set_run_font(p.add_run(title), 11, True, TEAL)
    p2 = doc.add_paragraph()
    set_run_font(p2.add_run(desc), 11, False, DARK)

# 11
add_heading_custom("11. Adoption roadmap (recommended)", 1)
add_table(
    ["Phase", "Focus", "Outcome"],
    [
        [
            "Phase 1",
            "Facility pilot: users, roles, Primary outreach, Triage→Lab→Consult→Pharmacy",
            "One complete digital patient day",
        ],
        [
            "Phase 2",
            "Add Dental / Eye secondary clinics + Stakeholders dashboard review cadence",
            "Specialty reach + executive visibility",
        ],
        [
            "Phase 3",
            "Deploy Field tablets for community sites (Deworming / ITN / registration)",
            "Coverage beyond facility walls",
        ],
        [
            "Phase 4",
            "Standard operating procedures, weekly KPI review, export for partners",
            "Sustainable adoption",
        ],
    ],
)

# 12
add_heading_custom("12. Closing message for stakeholders", 1)
add_body(
    "MedReach is not only a records system — it is an operating system for outreach days. "
    "It preserves the patient journey, equips community teams to work offline, and gives stakeholders "
    "the evidence they need to fund, supervise, and scale programs with confidence."
)
add_callout(
    "Next step",
    "Schedule a supervised pilot outreach day using one Primary program and one Secondary campaign. "
    "Review the Stakeholders dashboard the following morning with facility leadership.",
)

doc.add_paragraph()
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(
    p.add_run(
        "MedReach  ·  https://medreach.ecews.org  ·  Field: https://medreach.ecews.org/field"
    ),
    10,
    False,
    TEAL,
)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
set_run_font(p.add_run("Confidential — for stakeholder briefing use"), 9, False, MUTED)

# Appendix screenshot checklist
doc.add_page_break()
add_heading_custom("Appendix A — Screenshot checklist for your presentation", 1)
add_body("Capture these screens before the briefing to keep the story visual:")
add_table(
    ["Slide topic", "Where to open"],
    [
        ["Home / branding", "https://medreach.ecews.org/"],
        ["Care programs setup", "/admin/care-programs"],
        ["Outreach registration", "/patients/register-outreach"],
        ["Triage + referral", "/triage"],
        ["Patient visit journey", "/patient-dashboard/{patientId}"],
        ["Laboratory / Consult / Pharmacy queues", "/laboratory · /consultations · /pharmacy"],
        ["Secondary outreach (Deworming / ITN)", "/secondary-outreach"],
        ["Stakeholders dashboard", "/stakeholders"],
        ["Field tablet home / sync", "https://medreach.ecews.org/field · /sync"],
    ],
)

doc.save(out)
print(out)
print("bytes", os.path.getsize(out))
