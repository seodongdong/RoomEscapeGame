using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "VideoTapeDatabase", menuName = "Game/VideoTape Database")]
public class VideoTapeDatabase : ScriptableObject
{
	[System.Serializable]
	public class VideoData
	{
		public string tapeId;
		public int stageNumber;
		[TextArea(5, 15)]
		public string narration; // 범인의 나레이션

		[Header("Visual")]
		public Sprite thumbnailImage; // TV 화면에 표시될 이미지
	}

	[SerializeField] private List<VideoData> allVideos = new List<VideoData>();

	public VideoData GetVideo(string tapeId)
	{
		return allVideos.Find(v => v.tapeId == tapeId);
	}

	public VideoData GetVideoByStage(int stage)
	{
		return allVideos.Find(v => v.stageNumber == stage);
	}
}
