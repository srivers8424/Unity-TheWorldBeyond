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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldBeyondTutorial : MonoBehaviour
{
    static public WorldBeyondTutorial Instance = null;
    public Transform _canvasObject;
    public Image _labelBackground;
    public TextMeshProUGUI _tutorialText;
    // used for full-screen passthrough, only when player walks out of the room
    public MeshRenderer _passthroughSphere;
    bool _attachToView = false;
    // don't allow any other messages after this one, because the app is quitting
    bool _hitCriticalError = false;
    // when hitting a game-breaking error, quit the app after displaying the message
    const float _errorMessageDisplayTime = 5.0f;

    public enum TutorialMessage
    {
        EnableFlashlight, // hands only
        BallSearch,
        ShootBall,
        AimWall, // hands only
        ShootWall,
        SwitchToy,
        NoBalls,
        None,
        // Only Scene Error messages after this entry
        ERROR_USER_WALKED_OUTSIDE_OF_ROOM,    // runs every frame, to force on Passthrough. The only error that doesn't quit the app.
        ERROR_NO_SCENE_DATA,                  // by default, Room Setup is launched; if the user cancels, this message displays
        ERROR_USER_STARTED_OUTSIDE_OF_ROOM,   // user is outside of the room volume, likely from starting in a different guardian/room
        ERROR_NOT_ENOUGH_WALLS,               // fewer than 3 walls, or only non-walls discovered (e.g. user has only set up a desk)
        ERROR_TOO_MANY_WALLS,                 // a closed loop of walls was found, but there are other rooms/walls
        ERROR_ROOM_IS_OPEN                    // walls don't form a closed loop
    };
    public TutorialMessage _currentMessage { private set; get; } = TutorialMessage.BallSearch;

    private void Awake()
    {
        Instance = this;
        DisplayMessage(TutorialMessage.None);

        // ensure the UI renders above Passthrough and hands
        _passthroughSphere.material.renderQueue = 4499;
        _labelBackground.material.renderQueue = 4500;
        _tutorialText.fontMaterial.renderQueue = 4501;

        _passthroughSphere.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdatePosition();
    }

    public void DisplayMessage(TutorialMessage message)
    {
        if (_hitCriticalError)
        {
            return;
        }

        _canvasObject.gameObject.SetActive(message != TutorialMessage.None);

        _passthroughSphere.gameObject.SetActive(message == TutorialMessage.ERROR_USER_WALKED_OUTSIDE_OF_ROOM);

        AttachToView(message >= TutorialMessage.ERROR_USER_WALKED_OUTSIDE_OF_ROOM);

        _hitCriticalError = message >= TutorialMessage.ERROR_NO_SCENE_DATA;
        if (_hitCriticalError)
        {
            StartCoroutine(KillApp());
        }

        switch (message)
        {
            case TutorialMessage.EnableFlashlight:
                _tutorialText.text = "Open palm outward to enable flashlight";
                break;
            case TutorialMessage.BallSearch:
                _tutorialText.text = WorldBeyondManager.Instance._usingHands ?
                    "Search around your room for energy balls. Make a fist to grab them from afar." :
                    "Search around your room for energy balls. Aim and hold Index Trigger to absorb them.";
                break;
            case TutorialMessage.NoBalls:
                _tutorialText.text = "You're out of energy balls... switch back to the flashlight to find more";
                break;
            case TutorialMessage.ShootBall:
                _tutorialText.text = WorldBeyondManager.Instance._usingHands ?
                    "Shoot balls by opening fist" :
                    "Shoot balls with index trigger";
                break;
            case TutorialMessage.AimWall: // only used for hands
                _tutorialText.text = "With your palm facing up, aim at a wall";
                break;
            case TutorialMessage.ShootWall:
                _tutorialText.text = WorldBeyondManager.Instance._usingHands ?
                    "Close hand to open/close wall" :
                    "Shoot walls to open/close them";
                break;
            case TutorialMessage.SwitchToy:
                _tutorialText.text = "Use thumbstick left/right to switch toys";
                break;
            case TutorialMessage.None:
                break;
            case TutorialMessage.ERROR_USER_WALKED_OUTSIDE_OF_ROOM:
                _tutorialText.text = "Out of bounds. Please return to your room.";
                break;
            case TutorialMessage.ERROR_NO_SCENE_DATA:
                _tutorialText.text = "The World Beyond requires Scene data. Please run Room Setup in Settings > Guardian.";
                break;
            case TutorialMessage.ERROR_USER_STARTED_OUTSIDE_OF_ROOM:
                _tutorialText.text = "It appears you're outside of your room. Please enter your room and restart.";
                break;
            case TutorialMessage.ERROR_NOT_ENOUGH_WALLS:
                _tutorialText.text = "You haven't set up enough walls. Please run Room Setup in Settings > Guardian.";
                break;
            case TutorialMessage.ERROR_TOO_MANY_WALLS:
                _tutorialText.text = "Somehow, you have more walls than you should. Please redo your walls in Room Setup in Settings > Guardian.";
                break;
            case TutorialMessage.ERROR_ROOM_IS_OPEN:
                _tutorialText.text = "The World Beyond requires a closed space. Please redo your walls in Room Setup in Settings > Guardian.";
                break;
        }
        _currentMessage = message;
    }

    public void HideMessage(TutorialMessage message)
    {
        if (_currentMessage == message)
        {
            _canvasObject.gameObject.SetActive(false);
        }
    }

    void AttachToView(bool doAttach)
    {
        _attachToView = doAttach;
        // snap it to the view
        if (doAttach)
        {
            UpdatePosition(false);
        }

        // for now, attaching to the view is a special case and only used when there is no Scene detected
        // the black fade sphere needs to render before the UI
        if (WorldBeyondManager.Instance)
        {
            WorldBeyondManager.Instance._fadeSphere.sharedMaterial.renderQueue = doAttach ? 4497 : 4999;
        }
    }

    void UpdatePosition(bool useSmoothing = true)
    {
        Transform centerEye = WorldBeyondManager.Instance._mainCamera.transform;

        float smoothing = useSmoothing ? Mathf.SmoothStep(0.3f, 0.9f, Time.deltaTime / 50.0f) : 1.0f;

        Vector3 targetPosition = _attachToView ? centerEye.position + centerEye.forward * 0.7f : WorldBeyondManager.Instance.GetControllingHand(19).position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing);

        Vector3 lookDir = transform.position - centerEye.position;
        if (lookDir.magnitude > Mathf.Epsilon)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothing);
        }
    }

    IEnumerator KillApp()
    {
        yield return new WaitForSeconds(_errorMessageDisplayTime);
        Application.Quit();
    }
}
