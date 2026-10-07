using System;

/// <summary>
/// 雇用操作をModelとViewの間で調停するPresenter。
/// </summary>
public sealed class RecruitmentPresenter
{
    public event Action<CharacterInstance> CharacterGenerated;

    public CharacterInstance RecruitOne()
    {
        CharacterInstance character = _generator.Generate();

        if (character == null)
        {
            _view.ShowRecruitmentError(
                "雇用可能なキャラクターを生成できませんでした。");
            return null;
        }

        if (!_roster.Add(character))
        {
            _view.ShowRecruitmentError(
                "キャラクターを所持一覧へ追加できませんでした。");
            return null;
        }

        CharacterGenerated?.Invoke(character);
        _view.ShowGeneratedCharacter(character);
        return character;
    }

    public RecruitmentPresenter(
        CharacterGenerator generator,
        CharacterRosterModel roster,
        IRecruitmentView view)
    {
        _generator = generator ??
            throw new ArgumentNullException(nameof(generator));
        _roster = roster ??
            throw new ArgumentNullException(nameof(roster));
        _view = view ??
            throw new ArgumentNullException(nameof(view));
    }

    private readonly CharacterGenerator _generator;
    private readonly CharacterRosterModel _roster;
    private readonly IRecruitmentView _view;
}
