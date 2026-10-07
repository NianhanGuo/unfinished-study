// Shared first-playtest settings. Durations are seconds, distances are Unity metres.
public static class StudyTuning {
 public const float ObserveSeconds=.9f, MemorySeconds=5, ObservationGrace=.25f;
 public static readonly float[] DrawingSeconds={26,28,30};
 public const float FlankSpeed=.9f, WorkRange=1.45f, WorkAngle=28;
 public static readonly float[] DesignSeconds={9,12,4};
 public const float WalkSpeed=3.4f, SprintSpeed=6.4f, CrouchSpeed=2.2f;
 public const float StaminaCapacity=4, StaminaDelay=.8f, StaminaRecovery=.8f;
 public const float SightRange=10, SightHalfAngle=60, HearingRange=10;
 public const float FirstWarning=1.8f, Warning=1.2f, ChaseSpeed=5.1f, CaptureRadius=.6f;
}
