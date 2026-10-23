# A1a. The voice file named by each intro, checked at startup

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Content format", "Tunables and placeholders" (voice). ADR 0018.

- **A1a. The voice file named by each intro, checked at startup; one generated placeholder voice** (Core + Data + content, about 150 lines; decision 160)
  - **Goal:**
    - Each intro names a voice file in `content/audio/voices/`. All of them name `Placeholder.wav` for now.
    - At startup, a missing file or a name differing only in case stops the start with the file named, as missing art does.
    - Nothing plays yet (A1b).
  - **New types (Data):**
    - `Audio/AudioFolders` (static): where sound lives.
      - `RootIn(string contentRoot)` gives `content/audio`;
      - `Voices = "voices"`.
    - `Audio/AudioReferenceCheck` (static): every voice file a cinematic names exists by exact name.
      - `FindProblems(ContentCatalog catalog, string audioRoot)` returns one line per problem, "Named by cinematic 'ragnir_intro'".
      - It reuses `ArtReferenceCheck`'s case-mismatch logic. If that means copying `CaseMismatch`, move it into a shared internal type in this slice instead.
  - **Changes:**
    - Core `Cinematics/CinematicDefinition.VoiceFile` (required).
    - Data `Cinematics/CinematicCatalogValidator`: `voiceFile must not be empty.`
    - Game `Program.FirstScene` reports art and audio problems together.
    - Content:
      - `cinematics.json`: every intro gets `"voiceFile": "Placeholder.wav"`.
      - `content/audio/voices/Placeholder.wav`: about 3 s, made once with Windows' built-in speech synthesizer from Windows PowerShell 5.1. It is a tool already on the machine, and nothing is added to the solution. Record the command in the Done note:
        ```powershell
        Add-Type -AssemblyName System.Speech
        $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
        $format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)
        $synth.SetOutputToWaveFile("content\audio\voices\Placeholder.wav", $format)
        $synth.Speak("Placeholder voice-over.")
        $synth.Dispose()
        ```
    - `Jrpg.Game.csproj` already publishes `content/**`, so no change.
  - **Tests:**
    - `AudioReferenceCheckTests`:
      - `FindProblems_RepositoryContent_IsEmpty`
      - `FindProblems_MissingVoice_NamesTheCinematic`
      - `FindProblems_CaseMismatch_NamesTheActualFile`
    - `CinematicCatalogValidatorTests`: theory row for a blank `voiceFile`.
  - **Verify by hand:**
    - The game starts.
    - Renaming the file to `placeholder.wav` stops the start, naming both spellings.
    - The file plays in Windows' media player.
