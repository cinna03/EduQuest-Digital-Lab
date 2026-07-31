# Light & Life — why this needs AR (not just Unity UI)

## The rule
If a student can finish the lab by only clicking buttons in a dark room with the camera covered, **the design failed**.

## What the student controls
| Guide says | Student does in the real world | Lab reaction |
|------------|--------------------------------|--------------|
| Place on table | Anchors seedling on the lab table | Plant appears in their space |
| Find LIGHT | Point device at window / lamp | Meter → BRIGHT → photosynthesis / O₂ / energy |
| Find DARKNESS | Cover lens or face a dark corner | Meter → DARK → night mode |

There is **no “Add light” slider** that completes the experiment.

## Tech
- Live **webcam / device camera** samples brightness (`WorldLightSensor`)
- Same design maps to **AR Foundation light estimation** on phone builds later

## Build
**EduQuest → Build Light Lab Scene (AR)**

## Pitch line for graders
“The physical environment is the controller. AR isn’t a skin on a desktop sim — without real light and dark, the biology loop does not advance.”
