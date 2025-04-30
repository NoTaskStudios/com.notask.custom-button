# Custom Button Unity Package
Custom Button is a flexible and extensible Unity UI Button replacement that supports color tint, sprite swap, and animation transitions for button states. Built as a Unity Package, it integrates seamlessly with the Unity Package Manager.

## Features

- Color tint transition for Normal, Highlighted, Pressed, Selected, and Disabled states.
- Sprite swap transition for UI Image components.
- Animation transitions using customizable ScriptableObject presets.
- Default animation presets included: Move, Scale, Rotate, Shake, Resize, and Multi Animation.
- Support for child graphics and TextMesh Pro text color transitions.
- Easy to extend with custom animation presets.
- Sample scenes demonstrating usage.

## Requirements

- Unity Editor 2023.1 (23f1) or later.
- Unity UI package (`com.unity.ugui`).
- TextMesh Pro (`com.unity.textmeshpro`).
- (Optional) Universal Render Pipeline (`com.unity.render-pipelines.universal`).

## Installation

### Clone and Open

1. Clone or download this repository.
2. In Unity Hub, click **Add**, then select the project folder.

### As a standalone package

1. Copy the `Assets/Package` folder into your project's `Packages` directory.
2. Rename the folder to `com.oxentegames.custom-button`.
3. In your project's `Packages/manifest.json`, add:

   ```json
   "com.oxentegames.custom-button": "file:com.oxentegames.custom-button"
   ```

4. Save and return to Unity; the package will appear in the Package Manager.

## Usage

1. Create or select a UI element with an Image component (e.g., a Button).
2. Add the `CustomButton.CustomButtonClass` component.
3. Assign the **Target Graphic** field (defaults to the Image on the GameObject).
4. Enable and configure transitions:
   - Color Tint
   - Sprite Swap
   - Animation (drag preset assets from `Assets/Package/Runtime/Resources/DefaultPresets`).
5. (Optional) Add sub-transitions for additional graphics or texts.

### Code Example

```csharp
using CustomButton;
using UnityEngine;

public class Example : MonoBehaviour
{
    public CustomButtonClass customButton;

    void Start()
    {
        customButton.onClick.AddListener(OnButtonClicked);
        customButton.onStateChange += state => Debug.Log("State changed to: " + state);
    }

    private void OnButtonClicked()
    {
        Debug.Log("Custom Button Clicked!");
    }
}
```

## Samples

Sample scenes are included:

- `Assets/Package/Samples/Overview/sample-CustomButton.unity`

To import samples:
1. Open the Package Manager (`Window > Package Manager`).
2. Find **Custom Button**.
3. Click **Import Samples**.

## Extending with Custom Presets

To create a custom animation preset:

1. In the Project window, right-click: **Create > Custom Button > Presets > [PresetType]**.
2. Configure the preset parameters in the Inspector.
3. Assign the preset to your Custom Button component.

## Contributing

Contributions are welcome. Please open issues and submit pull requests.
