---
name: audio-separator
description: >-
  Architecture, contracts, and authoring workflow for the AudioSeparator
  modular audio stem separation library. Use when implementing pipeline,
  ONNX backends, I/O extensions, or Cursor rules for this repo.
---

# AudioSeparator

Modular .NET audio stem separator. The core **returns** separated audio; it never writes files.

## Layer map

```
Abstractions  ← contracts only, zero NuGet deps
Core          ← pipeline, session API, tasks
Onnx          ← InferenceSession, OnnxContext, abstract hooks (ReadInferenceSpec, CreateInferenceTask)
Onnx.Demucs   ← DemucsInferenceSpecReader, DemucsInferenceTask, htdemucs defaults
FFMPEG/NAudio ← IAudioReader + write extensions (optional persistence)
Examples      ← compose separate + write
```

Dependency rule: I/O plugins reference **Abstractions only**. Core never references FFMPEG, NAudio, or writers.

## Core principle

- `CreateSession(path|stream)` → probe + validate + expose `Tasks`
- `RunAsync()` → `SeparationResult` with PCM + optional encoded streams via writer
- `OpenStemPcmStream` / `OpenStemEncodedStreamAsync` for raw PCM vs WAV/MP3 bytes
- Persistence: `result.WriteToDirectoryAsync(dir)` in FFMPEG/NAudio extensions (optional writer)

## Key types

| Type | Source |
|------|--------|
| `InferenceSpec` | ONNX session introspection (names, dims, stems — **no sample rate**) |
| `AudioSourceInfo` | `IAudioReader.ProbeAsync` |
| `SeparationRequirements` | Builder: `SampleRate`, `StemNames` |
| `SeparationProcessingOptions` | Builder: `EnableOverlapAdd`, `OverlapRatio`, `OutputStemName` |
| `StemAudio` / `SeparationResult` | Pipeline output (`StemAudio.Audio` is a ready PCM stream) |

## Session flow

1. `var session = await separator.CreateSession(inputPath)` (or `CreateSession(stream, sourceInfo)`)
2. Attach progress callbacks on `session.Tasks`
3. `using var result = await session.RunAsync()`
4. Raw PCM: `result.OpenStemPcmStream("vocals")` or `CopyStemPcmToAsync` → `.pcm`
5. Encoded (WAV/MP3): `await result.OpenStemEncodedStreamAsync("vocals")` or `WriteToDirectoryAsync`

## Stem selection and overlap

- `.WithOutputStem("vocals")` — only that stem appears in `SeparationResult.Stems`
- `.WithOverlapAdd(enabled: true)` — overlap-add stitching (default `false`; increases inference time and CPU/GPU usage)

## I/O split

- Builder: `.UseReader(...)` or `.UseNAudio()` / `.UseFFMPEG()` (reader + optional writer)
- Writer: used only when saving to disk via `WriteToDirectoryAsync`

## Probe vs ReadAsync

- `ProbeAsync` runs once at session creation (validation, chunk count)
- `ReadAsync` is a single data pass inside `AudioReadTask`
- No `AudioMetadataTask` in the pipeline

## Rule authoring workflow

All committed `.cursor/rules/` and `.cursor/skills/` files must be **EN-US**.

1. Describe rule intent in chat (PT-BR is fine)
2. Review proposed EN-US rule text in Ask mode
3. Agent writes `.mdc` file in EN-US
4. Pre-commit: no Portuguese in repo artifacts; no "read entire project" rules; keep `alwaysApply` rules under ~30 lines

## Quota tips

- Use `@` specific files instead of full-repo exploration
- One rule per Ask session
- File-scoped rules over large always-on rules
