using UnityEngine;

public class SaveSystem : MonoBehaviour, ISaveSystem
{
    // 저장 키
    private const string SAVE_KEY = "GameSave";

    // 게임 데이터 저장
    public void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);     // 데이터 직렬화
        PlayerPrefs.SetString(SAVE_KEY, json);      // 저장
        PlayerPrefs.Save();                         // 저장 적용
        Debug.Log("게임 저장 완료");
    }

    // 게임 데이터 로드
    public GameData LoadGame()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))           // 저장된 데이터가 있는지 확인
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);          // 데이터 불러오기
            GameData data = JsonUtility.FromJson<GameData>(json);   // 데이터 역직렬화
            Debug.Log("게임 로드 완료");
            return data;
        }

        Debug.Log("저장 파일 없음");
        return null;
    }

    // 저장 파일 존재 여부 확인
    public bool HasSaveFile()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);        // 저장된 데이터가 있는지 확인
    }

    // 저장 파일 삭제
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);            // 저장된 데이터 삭제 
        PlayerPrefs.Save();                         // 저장 적용å
        Debug.Log("저장 파일 삭제");
    }
}