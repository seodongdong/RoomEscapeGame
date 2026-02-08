using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 비디오 나레이션 데이터베이스
/// 각 스테이지별 비디오테이프 내용 관리
/// </summary>
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

#if UNITY_EDITOR
	[ContextMenu("Add Sample Videos")]
	private void AddSampleVideos()
	{
		allVideos.Clear();

		// 1스테이지 샘플
		allVideos.Add(new VideoData
		{
			tapeId = "tape_stage1",
			stageNumber = 1,
			narration = "얘들아, 주방은 위험하니까 거실에서 놀고 있어라.\n" +
					   "아빠가 맛있는 거 만들어줄게."
		});

		// 2스테이지 샘플
		allVideos.Add(new VideoData
		{
			tapeId = "tape_stage2",
			stageNumber = 2,
			narration = "우리 딸... 어디 갔니...\n" +
					   "아빠가 미안해... 아빠가 지켜주지 못해서..."
		});

		Debug.Log("샘플 비디오 추가 완료!");
	}
#endif
}