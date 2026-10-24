# A1a. The voice step

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Content format", "Tunables and placeholders" (voice). ADRs 0016, 0018.

- **A1a. The voice step: an event starts a voice file, checked at startup; one generated placeholder voice; the editor authors it** (Core + Data + Editor + content, about 220 lines; decision 160)
  - **Goal:**
    - An event step `{ "type": "voice", "file": "Placeholder.wav" }` starts a voice-over and doesn't wait: the stills that follow play over it.
    - It is a presentation step, so a skip runs past it (K2a).
    - At startup, a missing file, or a name differing only in case, stops the start with the file named, as missing art does.
    - Each leader's intro starts with the placeholder voice. Nothing plays yet (A1b).
  - **New types:**
    - Core `Events/VoiceStep(string File) : EventStep`, which starts a voice-over in `audio/voices/` and runs on at once (`IsPresentation`).
    - Data `Audio/AudioFolders` (static): where sound lives. `RootIn(string contentRoot)` gives `content/audio`, and `Voices = "voices"`.
    - Data `Audio/AudioReferenceCheck` (static): every voice step's file exists by exact name, "Named by event 'intro_omer' on map 'intro'". It reuses `ArtReferenceCheck`'s case-mismatch logic; if that means copying `CaseMismatch`, move it into a shared internal type in this slice instead.
  - **Changes:**
    - Core `EventRun`: a voice step runs on at once, like a swap, and records it so the Game can play it. `ExplorationState.StartedVoice` is "the voice the running event started, or null", cleared when the event ends or skips.
    - Data:
      - `ContentTypeDiscriminators` gains `voice`;
      - `MapEventValidator` checks `file` not blank;
      - `Program.FirstScene` reports art and audio problems together (Game).
    - Editor: `EventStepKind.Voice` with a file field, its `From`/`ToStep` cases and its label ("Voice Placeholder.wav").
    - Content:
      - `content/maps/intro`: every intro event starts with `{ "type": "voice", "file": "Placeholder.wav" }`.
      - `content/audio/voices/Placeholder.wav`: about 3 s, made once with Windows' built-in speech synthesizer from Windows PowerShell 5.1. It is a tool already on the machine, and nothing is added to the solution. Record the command in the Done note:
        ```powershell
        Add-Type -AssemblyName System.Speech
        $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
        $format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)
        $synth.SetOutputToWaveFile("content\audio\voices\Placeholder.wav", $format)
        $synth.Speak("Placeholder voice-over.")
        $synth.Dispose()
        ```
    - `Jrpg.Game.csproj` already publishes `content/**`, so no change there.
  - **Tests:**
    - `EventRunTests`:
      - `Start_VoiceThenStill_RunsOnToTheStillWithTheVoiceStarted`
      - `SkipPresentation_VoiceAndStills_StopsOnTheWarp`
    - `AudioReferenceCheckTests`:
      - `FindProblems_RepositoryContent_IsEmpty`
      - `FindProblems_MissingVoice_NamesTheEventAndMap`
      - `FindProblems_CaseMismatch_NamesTheActualFile`
    - `MapEventValidatorTests`: theory row for a blank `file`.
    - `EventStepViewModelTests`: `From_VoiceStep_KeepsTheFile`.
  - **Verify by hand:**
    - The game starts.
    - Renaming the file to `placeholder.wav` stops the start, naming both spellings.
    - The file plays in Windows' media player.
    - The editor opens the intro map and shows the voice step.
