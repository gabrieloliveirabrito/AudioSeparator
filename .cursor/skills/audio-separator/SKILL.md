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
Onnx          ← InferenceSession, InferenceSpec, OnnxInferenceTask
Onnx.Demucs   ← Demucs ONNX backend (one model implementation)
FFMPEG/NAudio ← IAudioReader + write extensions (optional persistence)
Examples      ← compose separate + write
```

Dependency rule: I/O plugins reference **Abstractions only**. Core never references FFMPEG, NAudio, or writers.

## Core principle

- `CreateSession(path)` → probe + validate + expose `Tasks`
- `RunAsync()` → `SeparationResult` with `StemAudio` per stem
- Persistence: `result.WriteToDirectoryAsync(dir, writer)` in FFMPEG/NAudio extensions

## Key types

| Type | Source |
|------|--------|
| `InferenceSpec` | ONNX session introspection (names, dims, stems — **no sample rate**) |
| `AudioSourceInfo` | `IAudioReader.ProbeAsync` |
| `SeparationRequirements` | Builder: `SampleRate`, `StemNames` |
| `StemAudio` / `SeparationResult` | Pipeline output |

## Session flow

1. `var session = await separator.CreateSession(inputPath)`
2. Attach progress callbacks on `session.Tasks`
3. `var result = await session.RunAsync()`
4. `await result.WriteToDirectoryAsync("./Outputs", writer)` (consumer side)

## I/O split

- Builder: `.UseReader(...)` via `.UseNAudio()` or `.UseFFMPEG()`
- Writer: `NAudioExtensions.CreateWriter()` / `FFMPEGExtensions.CreateWriter()` — used only when saving

## Probe vs ReadAsync

- `ProbeAsync` runs once at session creation (validation, chunk count)
- `ReadAsync` is a single data pass inside `AudioReadTask`
- No `AudioMetadataTask` in the pipeline

## Known gaps

- Streaming stem output (`StemAudio.ToStream()`) is future work
- `@Older/` is legacy reference — only read when explicitly mentioned

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
