namespace Renaissance.Domain.Enums;

public enum CareProgramStatus
{
    Draft = 0,
    Active = 1,
    Ended = 2
}

public enum PatientIdMode
{
    AutoSerial = 0,
    PreGenerated = 1
}

public enum ProgramPatientIdStatus
{
    Available = 0,
    Registered = 1,
    Voided = 2
}

public enum CareProgramType
{
    Primary = 0,
    Secondary = 1
}
