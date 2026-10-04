using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Dialogue;

public static class CommunityDialogueCatalog
{
    private sealed record CommunityVoice(
        string Id,
        string Prefix,
        string Day,
        string QuestActive,
        string PredatorGone,
        string ApparitionGone,
        string SafeAfterBoth,
        string Rain,
        string Night);

    private static readonly CommunityVoice[] Voices =
    [
        new(
            "settler-farmer-01",
            "com.farmer01",
            "Ziemia przy lesie jest ciężka, ale rodzi, jeśli jej nie pospieszać.",
            "Od mokradeł wraca mniej ludzi. Dobrze, że ktoś wreszcie sprawdza, co się tam dzieje.",
            "Skoro bestii już nie ma, łatwiej będzie wyprowadzać bydło dalej od palisady.",
            "Nocne światło podobno ucichło. Ludzie jeszcze długo będą omijać tamtą drogę.",
            "Jeśli i światło ucichło, i bestia padła, może znów zaczniemy chodzić do starej przeprawy.",
            "Taki deszcz nakarmi pole, ale drogę do mokradeł zamieni w błoto.",
            "O tej porze wolę słyszeć własne kroki niż coś w trzcinach."),

        new(
            "settler-farmer-02",
            "com.farmer02",
            "Rano najlepiej widać, które grządki przetrwały noc.",
            "Dzieciom kazaliśmy nie chodzić na wschód od wsi, dopóki sprawa się nie wyjaśni.",
            "Mówią, że drapieżnik nie wróci. Oby mieli rację.",
            "Światło z mokradeł znikło. Dla mnie to wystarczy, żeby tej nocy lepiej spać.",
            "Wreszcie jest ciszej. Może za kilka dni ludzie przestaną patrzeć na drogę jak na grób.",
            "W deszcz wszystko pachnie mocniej. Nawet dym z paleniska trzyma się przy ziemi.",
            "Nocą wieś brzmi inaczej. Słychać rzeczy, których za dnia nikt nie zauważa."),

        new(
            "settler-woodworker-01",
            "com.woodworker",
            "Dobre drewno poznaje się po słojach, nie po kolorze kory.",
            "Jeśli przeprawa ma wrócić do użytku, będę miał robotę przy deskach i podporach.",
            "Po takiej bestii zostają nie tylko ślady. Trzeba sprawdzić każdą uszkodzoną belkę.",
            "Skoro nocne zjawisko ustało, można będzie pracować dłużej bez oglądania się za siebie.",
            "Najpierw naprawimy kładkę. Potem dopiero uwierzę, że sprawa naprawdę się skończyła.",
            "Mokre drewno pracuje inaczej. W taką pogodę niczego ważnego nie składam na gotowo.",
            "Po ciemku nie ocenia się pęknięcia w belce. Po ciemku wraca się do domu."),

        new(
            "settler-potter-01",
            "com.potter",
            "Glina z doliny jest tłusta. Ta spod mokradeł pęka, jeśli źle ją wysuszyć.",
            "Ludzie kupują teraz więcej naczyń na zapasy. Strach zawsze zmienia handel.",
            "Dobrze, że to coś już nie chodzi po mokradle. Zwierzęta wyczuwały je wcześniej niż my.",
            "Jeżeli światło zgasło na dobre, może skończą się opowieści szeptane przy piecu.",
            "Cisza po wszystkim jest dziwna. Ale wolę ją od krzyku i plotek.",
            "W taki deszcz piec musi trzymać równy ciąg, inaczej cała partia pójdzie na straty.",
            "Nocą pilnuję pieca. Przynajmniej ogień daje coś pewnego do patrzenia."),

        new(
            "settler-trader-01",
            "com.trader",
            "Towar jest wart tyle, ile ktoś zapłaci, ale droga decyduje, czy w ogóle tu dotrze.",
            "Dopóki przeprawa jest niepewna, wszystko przychodzi później i kosztuje więcej.",
            "Śmierć drapieżnika to dobra wiadomość dla kupców. Złą wiadomością jest to, co zrobił z drogą.",
            "Jeśli nocne światło naprawdę zniknęło, woźnice może przestaną odmawiać późnych przejazdów.",
            "Bez bestii i bez światła handel wróci szybciej niż zaufanie ludzi.",
            "Deszcz zatrzymuje wozy bardziej skutecznie niż strażnik przy bramie.",
            "Po zmierzchu interesy robi się tylko z kimś, kogo już się zna."),

        new(
            "settler-carrier-01",
            "com.carrier",
            "Najcięższy nie jest worek. Najgorsza jest droga, na której nie ma gdzie postawić nogi.",
            "Przez mokradła teraz nie noszę niczego, czego nie umiałbym porzucić i uciec.",
            "Jeśli drapieżnik padł, mogę znów brać krótszą drogę. Po naprawie przeprawy.",
            "Światło znikło? Dobrze. Nie lubię rzeczy, których nie da się odepchnąć ramieniem.",
            "Jak naprawią przejście, pierwszy sprawdzę, czy da się tamtędy przejść z pełnym ładunkiem.",
            "Mokry worek waży dwa razy tyle, a człowiek przeklina trzy razy częściej.",
            "Nocą niczego nie noszę poza drewnem do paleniska."),

        new(
            "settler-elder-01",
            "com.elder",
            "Wieś pamięta więcej, niż zapisuje. Problem w tym, że pamięć ludzi lubi poprawiać fakty.",
            "Słuchaj świadków, ale nie składaj ich opowieści w jedną prawdę zbyt wcześnie.",
            "Bestia była cielesna. To dobrze wiedzieć. Nie znaczy to jeszcze, że wyjaśnia wszystko.",
            "Jeśli pomogłeś temu, co zostało po zaginionym, nie rób z tego większej opowieści, niż potrzeba.",
            "Dwie przyczyny, dwa rozwiązania. Takie odpowiedzi są mniej wygodne, ale zwykle bliższe prawdzie.",
            "Deszcz ucisza wieś i wydobywa zapach ziemi. Dobrze wtedy słuchać, zanim się mówi.",
            "Stary człowiek nocą śpi krótko. Dzięki temu słyszy, ile strachu mieści się w ciszy."),

        new(
            "settler-traveler-01",
            "com.traveler",
            "Przyjechałem od zachodu. O waszych mokradłach słyszałem już pół dnia drogi stąd.",
            "Na trakcie mówią, że tutaj znika się przy świetle. Plotka biegnie szybciej niż wóz.",
            "Wieść o zabitej bestii też pójdzie w świat. Pewnie bardziej urosła, zanim dotrę do następnej wsi.",
            "Jeśli uspokoiłeś zjawę, pilnuj, kto będzie opowiadał tę historię. Ludzie dodają własne zakończenia.",
            "To dobra historia do opowiadania przy drodze: jedno miejsce, dwa zagrożenia i człowiek, który ich nie pomylił.",
            "W deszcz zostanę tu dłużej. Koła ugrzęzną szybciej niż ja zdążę pożałować wyjazdu.",
            "Podróżny po zmroku liczy ogniska. Każde następne oznacza, że droga jeszcze żyje.")
    ];

    private static readonly IReadOnlyDictionary<string, CommunityVoice> VoiceById =
        Voices.ToDictionary(voice => voice.Id, StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, DialogueGraph> Graphs =
        Voices.ToDictionary(
            voice => voice.Id,
            BuildGraph,
            StringComparer.Ordinal);

    public static bool HasGraph(string npcId) =>
        Graphs.ContainsKey(npcId);

    public static bool TryGetGraph(
        string npcId,
        out DialogueGraph graph) =>
        Graphs.TryGetValue(npcId, out graph!);

    public static string SelectStartNode(
        WorldState world,
        string npcId)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!VoiceById.TryGetValue(npcId, out var voice))
            throw new KeyNotFoundException(
                $"No R0 community dialogue profile for NPC '{npcId}'.");

        var quest =
            world.Progress.Quests.Get(
                VerticalSliceBootstrap.ContractQuestId);

        var released =
            world.Progress.HasFlag(
                VerticalSliceRituals.ReleasedFlag);
        var predatorGone =
            SwampPredatorEncounter.IsKilled(world);

        if (quest.Phase is QuestPhase.Resolved or QuestPhase.TurnedIn)
        {
            if (released && predatorGone)
                return $"{voice.Prefix}.safe";
            if (released)
                return $"{voice.Prefix}.apparition-gone";
            if (predatorGone)
                return $"{voice.Prefix}.predator-gone";
        }

        if (world.Weather.Condition is WeatherKind.Rain or WeatherKind.Storm ||
            world.Weather.RainIntensity >= 0.35f)
        {
            return $"{voice.Prefix}.rain";
        }

        if (quest.Phase is
            QuestPhase.Active or
            QuestPhase.Investigation or
            QuestPhase.Preparation or
            QuestPhase.Encounter)
        {
            return $"{voice.Prefix}.quest";
        }

        var hour = world.Time.TimeOfDayHours;
        if (hour >= 20d || hour < 6d)
            return $"{voice.Prefix}.night";

        if (predatorGone)
            return $"{voice.Prefix}.predator-gone";
        if (released)
            return $"{voice.Prefix}.apparition-gone";

        return $"{voice.Prefix}.day";
    }

    private static DialogueGraph BuildGraph(
        CommunityVoice voice) =>
        new(
            $"dialogue.community.{voice.Id}",
            $"{voice.Prefix}.day",
            [
                AmbientNode(
                    $"{voice.Prefix}.day",
                    voice.Id,
                    voice.Day),
                AmbientNode(
                    $"{voice.Prefix}.quest",
                    voice.Id,
                    voice.QuestActive),
                AmbientNode(
                    $"{voice.Prefix}.predator-gone",
                    voice.Id,
                    voice.PredatorGone),
                AmbientNode(
                    $"{voice.Prefix}.apparition-gone",
                    voice.Id,
                    voice.ApparitionGone),
                AmbientNode(
                    $"{voice.Prefix}.safe",
                    voice.Id,
                    voice.SafeAfterBoth),
                AmbientNode(
                    $"{voice.Prefix}.rain",
                    voice.Id,
                    voice.Rain),
                AmbientNode(
                    $"{voice.Prefix}.night",
                    voice.Id,
                    voice.Night)
            ]);

    private static DialogueNode AmbientNode(
        string id,
        string speakerId,
        string text) =>
        new(
            id,
            speakerId,
            text,
            [
                new DialogueChoice(
                    $"{id}.leave",
                    "Do zobaczenia.",
                    null,
                    [],
                    [])
            ]);
}
