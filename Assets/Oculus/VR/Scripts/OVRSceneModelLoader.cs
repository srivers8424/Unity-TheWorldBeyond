/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;

/// <summary>
/// Utility for loading a scene model. Derive from this class to customize the scene loading behavior and respond to
/// events.
/// </summary>
[RequireComponent(typeof(OVRSceneManager))]
public class OVRSceneModelLoader : MonoBehaviour
{
  /// <summary>
  /// The <see cref="OVRSceneManager"/> component that this loader will use.
  /// </summary>
  protected OVRSceneManager SceneManager { get; private set; }

  void Start()
  {
    SceneManager = GetComponent<OVRSceneManager>();

    // Bind the events associated with LoadSceneModel()
    SceneManager.SceneModelLoadedSuccessfully += OnSceneModelLoadedSuccessfully;
    SceneManager.NoSceneModelToLoad += OnNoSceneModelToLoad;

    // Bind the events associated with RequestSceneCapture()
    SceneManager.SceneCaptureReturnedWithoutError += OnSceneCaptureReturnedWithoutError;
    SceneManager.UnexpectedErrorWithSceneCapture += OnUnexpectedErrorWithSceneCapture;

    OnStart();
  }

  /// <summary>
  /// Invoked from this component's `Start` method. The default behavior is to load the scene model using
  /// <see cref="OVRSceneManager.LoadSceneModel"/>.
  /// </summary>
  protected virtual void OnStart()
  {
    // Load the scene
    SceneManager.Verbose?.Log(nameof(OVRSceneModelLoader),
    $"{nameof(OnStart)}() calling {nameof(OVRSceneManager)}.{nameof(OVRSceneManager.LoadSceneModel)}()");
    SceneManager.LoadSceneModel();
  }

  /// <summary>
  /// Invoked when the scene model has successfully loaded.
  /// </summary>
  protected virtual void OnSceneModelLoadedSuccessfully()
  {
    // The scene model was captured successfully. At this point all prefabs have been instantiated.
    SceneManager.Verbose?.Log(nameof(OVRSceneModelLoader),
    $"{nameof(OVRSceneManager)}.{nameof(OVRSceneManager.LoadSceneModel)}() completed successfully.");
  }

  /// <summary>
  /// Invoked when there is no scene model available. The default behavior requests scene capture using
  /// <see cref="OVRSceneManager.RequestSceneCapture"/>.
  /// </summary>
  protected virtual void OnNoSceneModelToLoad()
  {
    // There's no Scene model, we have to ask the user to create one
    SceneManager.Verbose?.Log(nameof(OVRSceneModelLoader),
    $"{nameof(OnNoSceneModelToLoad)}() calling {nameof(OVRSceneManager)}.{nameof(OVRSceneManager.RequestSceneCapture)}()");
    SceneManager.RequestSceneCapture();
  }

  /// <summary>
  /// Invoked when the scene capture succeeds without error. The default behavior loads the scene model using
  /// <see cref="OVRSceneManager.LoadSceneModel"/>.
  /// </summary>
  protected virtual void OnSceneCaptureReturnedWithoutError()
  {
    // The capture flow successfully returned, we can now load the scene model
    SceneManager.Verbose?.Log(nameof(OVRSceneModelLoader),
    $"{nameof(OnSceneCaptureReturnedWithoutError)}() calling {nameof(OVRSceneManager)}.{nameof(OVRSceneManager.LoadSceneModel)}()");
    SceneManager.LoadSceneModel();
  }

  /// <summary>
  /// Invoked when the scene capture encounters an unexpected error.
  /// </summary>
  protected virtual void OnUnexpectedErrorWithSceneCapture()
  {
    // An unexpected error was returned when invoking the capture flow. This prevents the user
    // from capturing their room.
    SceneManager.Verbose?.LogError(nameof(OVRSceneModelLoader),
    "Requesting the capture flow failed. The Scene Model cannot be loaded.");
  }
}
