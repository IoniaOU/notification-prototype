using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
	[Tooltip("Select inputfield in scene.")]
	public InputField inputField;

	[Tooltip("Select canvas in scene.")]
	public Canvas canvas;

	[Tooltip("Select notification panel that you want to send in prefab folder.")]
	public GameObject notificationPanelPrefab;
	private GameObject currentNotificationPanel; //This Gameobject variable for active notification.

	[Tooltip("Enter an URL for notification image.")]
	public string NotificationImageURL;

	[Tooltip("Select an SFX for notification")]
	public AudioClip notificationSfx;

	[Tooltip("Time between notifications in second unit")]
	public float bufferTime;
	private float _bufferTime = -1; //

	private Texture2D saved;

	[Tooltip("DO NOT CHANGE MANUALLY!")]
	public bool isThereNotification; //This controls that is there any active notification.

	List<Notification> notificationList = new List<Notification>(); //Notification order.

	private WWW www;
	private bool imageCached = false;
	private bool fileFound = true;

	void Awake()
	{
		saved = new Texture2D (72, 72);
		if (File.Exists (Application.dataPath + "/cachedimage.png"))
		{
			fileFound = true;
			saved.LoadImage (File.ReadAllBytes(Application.dataPath + "/cachedimage.png"));
		}
		else
		{
			fileFound = false;
			StartCoroutine (DownloadImage());
		}
	}

	IEnumerator DownloadImage()
	{
		www = new WWW(NotificationImageURL);
		yield return www;
		www.LoadImageIntoTexture (saved);
		www.Dispose ();
		www = null;
	}

	void Update()
	{
		if (!imageCached && !fileFound && www==null )
		{
			File.WriteAllBytes (Application.dataPath + "/cachedimage.png", saved.EncodeToPNG());
			imageCached = true;
		}
		
		if (notificationList.Count > 0)
		{
			if (!isThereNotification && _bufferTime < 0)
			{
				InstantiateNotification ();
				isThereNotification = true;
				_bufferTime = bufferTime;
			}
			else if (!isThereNotification && _bufferTime > 0)
			{
				_bufferTime -= Time.deltaTime;
			}
		}
		else
		{
			if (_bufferTime > 0)
			{
				_bufferTime -= Time.deltaTime;
			}
		}
	}

	public void InstantiateNotification()
	{
		GameObject currentNotificationPanel = Instantiate(notificationPanelPrefab) as GameObject;
		currentNotificationPanel.gameObject.transform.SetParent (canvas.gameObject.transform,false);
		currentNotificationPanel.gameObject.transform.GetChild (0).GetComponent<RawImage> ().texture = notificationList [0].notificationImage;
		currentNotificationPanel.gameObject.transform.GetChild (1).GetComponent<Text> ().text = notificationList [0].notificationText;
		currentNotificationPanel.gameObject.transform.GetComponent<NotificationController> ().notificationManager = this.gameObject;
		notificationList.RemoveAt (0);
	}

	public void RegisterNotification()
	{
		notificationList.Add (new Notification (saved, inputField.text));
	}
}
