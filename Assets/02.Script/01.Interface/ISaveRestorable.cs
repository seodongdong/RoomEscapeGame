/// <summary>
/// 저장/불러오기 복원 시, "이 오브젝트가 어떤 단서 ID를 나타내는지"와
/// "이미 획득된 상태라면 스스로를 비활성화"하는 방법을 제공하는 인터페이스.
///
/// UsableItemClue, DiaryClue 등 기존 단서 스크립트가 이 인터페이스를
/// 추가로 구현하면, SaveLoader가 별도 분기 없이 동일한 방식으로
/// "이미 획득된 단서 오브젝트를 다시 숨기는" 처리를 할 수 있습니다.
/// </summary>
public interface ISaveRestorable
{
	string RestoreItemId { get; }
	void ApplyAlreadyCollected();
}