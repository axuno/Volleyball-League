namespace TournamentManager.RoundRobin;

// Inspired by Pit Schneider's Ideal Round Robin
// on https://github.com/Schneipi/ideal-round-robin
/// <summary>
/// This generic class calculates all matches for a group of participants.
/// The round-robin system is applied, i.e. all participants in the group play each other.
/// Then the number of home/guest matches is optimized manually.
/// That's why only 5 to 14 participants are supported.
/// </summary>
/// <typeparam name="TP">The type of the participant objects. Objects must have <see cref="IEquatable{T}"/> implemented.</typeparam>
internal class IdealRoundRobinSystem<TP> : IRoundRobinSystem<TP> where TP : IEquatable<TP>
{
    #region *** Description of Ideal Round Robin ***

    /*
       Ideal Round Robin
       (Text published by Pit Schneider on 2021-06-21 on https://github.com/Schneipi/ideal-round-robin)

       Licensed under GNU General Public License v3.0

       Given n teams, every round robin schedule has the following properties:

       A) n is even

       1. Mirrored double round robin schedule: Same games in round k and round k+n-1,
          except inverted home advantage.
       2. Total number of breaks (consecutive home/away games) is a minimum and
          equals 3n-6 [de Werra 1981].
       3. First n-1 rounds on their own compose a single round robin schedule with
          a minimum of n-2 breaks [de Werra 1981] (odd team numbers have n/2 home games).
       4. No breaks in rounds 2 and n-1, entailing that every team has precisely 1 home
          game in the first 2 rounds and precisely 1 home game in the last 2 rounds.
       5. No consecutive breaks, entailing no more than 2 home/away games in a row for every team.
       6. Teams 1 and 2 have 0 breaks, all other teams have 3 breaks.
       7. The higher the team number, the lower the round number containing
          the first break for the team.

       b) n is odd

       1. Mirrored double round robin schedule: Same games in round k and round k+n,
          except inverted home advantage.
       2. Total number of breaks (consecutive home/away games without considering bye rounds)
          is a minimum and equals n [de Werra 1981].
       3. First n rounds on their own compose a single round robin schedule with a
          minimum of 0 breaks [de Werra 1981].
       4. An optimum amount of n-1 teams have precisely 1 home game in the first 2 rounds
          and precisely 1 home game in the last 2 rounds.
       5. No consecutive breaks, entailing no more than 2 home/away games in a row for every team.
       6. Every team has exactly 1 break.
       7. The higher the team number, the lower the round number containing the first break for the team.
     */

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="IdealRoundRobinSystem{TP}"/> class.
    /// </summary>
    /// <param name="participants">The collection of participants.</param>
    public IdealRoundRobinSystem(ICollection<TP> participants)
    {
        Participants = participants;
    }

    /// <inheritdoc/>
    public ICollection<TP> Participants { get; }

    /*
       round01		04-03	05-02
       round02		01-04	03-05
       round03		02-03	05-01
       round04		01-02	04-05
       round05		02-04	03-01
     */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants5 =
        new()
        {
            (1, 4, 3),
            (1, 5, 2),

            (2, 1, 4),
            (2, 3, 5),

            (3, 2, 3),
            (3, 5, 1),

            (4, 1, 2),
            (4, 4, 5),

            (5, 2, 4),
            (5, 3, 1)
        };

    /*
       round01		01-05	03-02	06-04
       round02		02-06	04-01	05-03
       round03		01-02	03-06	05-04
       round04		02-04	03-01	06-05
       round05		01-06	04-03	05-02
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants6 =
        new()
        {
            (1, 1, 5),
            (1, 3, 2),
            (1, 6, 4),

            (2, 2, 6),
            (2, 4, 1),
            (2, 5, 3),

            (3, 1, 2),
            (3, 3, 6),
            (3, 5, 4),

            (4, 2, 4),
            (4, 3, 1),
            (4, 6, 5),

            (5, 1, 6),
            (5, 4, 3),
            (5, 5, 2)
        };

    /*
        round01		05-04	06-03	07-02
        round02		01-06	03-05	04-07
        round03		02-04	05-01	07-03
        round04		01-07	03-02	06-05
        round05		02-01	04-03	07-06
        round06		01-04	05-07	06-02
        round07		02-05	03-01	04-06
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants7 =
        new()
        {
            (1, 5, 4),
            (1, 6, 3),
            (1, 7, 2),

            (2, 1, 6),
            (2, 3, 5),
            (2, 4, 7),

            (3, 2, 4),
            (3, 5, 1),
            (3, 7, 3),

            (4, 1, 7),
            (4, 3, 2),
            (4, 6, 5),

            (5, 2, 1),
            (5, 4, 3),
            (5, 7, 6),

            (6, 1, 4),
            (6, 5, 7),
            (6, 6, 2),

            (7, 2, 5),
            (7, 3, 1),
            (7, 4, 6)
        };

    /*
       round01		01-07	03-06	05-04	08-02
       round02		02-05	04-03	06-01	07-08
       round03		01-04	03-02	05-08	07-06
       round04		02-01	04-07	05-03	08-06
       round05		01-05	03-08	06-04	07-02
       round06		02-06	03-01	05-07	08-04
       round07		01-08	04-02	06-05	07-03
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants8 =
        new()
        {
            (1, 1, 7),
            (1, 3, 6),
            (1, 5, 4),
            (1, 8, 2),

            (2, 2, 5),
            (2, 4, 3),
            (2, 6, 1),
            (2, 7, 8),

            (3, 1, 4),
            (3, 3, 2),
            (3, 5, 8),
            (3, 7, 6),

            (4, 2, 1),
            (4, 4, 7),
            (4, 5, 3),
            (4, 8, 6),

            (5, 1, 5),
            (5, 3, 8),
            (5, 6, 4),
            (5, 7, 2),

            (6, 2, 6),
            (6, 3, 1),
            (6, 5, 7),
            (6, 8, 4),

            (7, 1, 8),
            (7, 4, 2),
            (7, 6, 5),
            (7, 7, 3)
        };

    /*
       round01		06-05	07-04	08-03	09-02
       round02		01-08	03-06	04-09	05-07
       round03		02-04	06-01	07-03	09-05
       round04		01-07	03-09	05-02	08-06
       round05		02-03	04-05	07-08	09-01
       round06		01-02	03-04	06-07	08-09
       round07		02-08	04-01	05-03	09-06
       round08		01-05	06-02	07-09	08-04
       round09		02-07	03-01	04-06	05-08
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants9 =
        new()
        {
            (1, 6, 5),
            (1, 7, 4),
            (1, 8, 3),
            (1, 9, 2),

            (2, 1, 8),
            (2, 3, 6),
            (2, 4, 9),
            (2, 5, 7),

            (3, 2, 4),
            (3, 6, 1),
            (3, 7, 3),
            (3, 9, 5),

            (4, 1, 7),
            (4, 3, 9),
            (4, 5, 2),
            (4, 8, 6),

            (5, 2, 3),
            (5, 4, 5),
            (5, 7, 8),
            (5, 9, 1),

            (6, 1, 2),
            (6, 3, 4),
            (6, 6, 7),
            (6, 8, 9),

            (7, 2, 8),
            (7, 4, 1),
            (7, 5, 3),
            (7, 9, 6),

            (8, 1, 5),
            (8, 6, 2),
            (8, 7, 9),
            (8, 8, 4),

            (9, 2, 7),
            (9, 3, 1),
            (9, 4, 6),
            (9, 5, 8)
        };

    /*
       round01		01-09	03-07	06-02	08-05	10-04
       round02		02-10	04-08	05-03	07-01	09-06
       round03		01-05	03-04	06-10	08-02	09-07
       round04		02-03	04-01	05-09	07-06	10-08
       round05		01-02	03-10	06-08	07-05	09-04
       round06		02-09	04-07	05-06	08-03	10-01
       round07		01-08	03-06	05-04	07-02	09-10
       round08		02-05	03-01	06-04	08-09	10-07
       round09		01-06	04-02	05-10	07-08	09-03
     */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants10 =
        new()
        {
            (1, 1, 9),
            (1, 3, 7),
            (1, 6, 2),
            (1, 8, 5),
            (1, 10, 4),

            (2, 2, 10),
            (2, 4, 8),
            (2, 5, 3),
            (2, 7, 1),
            (2, 9, 6),

            (3, 1, 5),
            (3, 3, 4),
            (3, 6, 10),
            (3, 8, 2),
            (3, 9, 7),

            (4, 2, 3),
            (4, 4, 1),
            (4, 5, 9),
            (4, 7, 6),
            (4, 10, 8),

            (5, 1, 2),
            (5, 3, 10),
            (5, 6, 8),
            (5, 7, 5),
            (5, 9, 4),

            (6, 2, 9),
            (6, 4, 7),
            (6, 5, 6),
            (6, 8, 3),
            (6, 10, 1),

            (7, 1, 8),
            (7, 3, 6),
            (7, 5, 4),
            (7, 7, 2),
            (7, 9, 10),

            (8, 2, 5),
            (8, 3, 1),
            (8, 6, 4),
            (8, 8, 9),
            (8, 10, 7),

            (9, 1, 6),
            (9, 4, 2),
            (9, 5, 10),
            (9, 7, 8),
            (9, 9, 3),

            (9, 1, 6),
            (9, 4, 2),
            (9, 5, 10),
            (9, 7, 8),
            (9, 9, 3)
        };

    /*
       round01		07-06	08-05	09-04	10-03	11-02
       round02		01-10	03-08	04-11	05-07	06-09
       round03		02-04	07-03	08-01	09-05	11-06
       round04		01-07	03-09	05-11	06-02	10-08
       round05		02-05	04-06	07-10	09-01	11-03
       round06		01-11	03-02	05-04	08-07	10-09
       round07		02-01	04-03	06-05	09-08	11-10
       round08		01-04	03-06	07-09	08-11	10-02
       round09		02-08	04-10	05-03	06-01	11-07
       round10		01-05	07-02	08-04	09-11	10-06
       round11		02-09	03-01	04-07	05-10	06-08
     */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants11 =
        new()
        {
            (1, 7, 6),
            (1, 8, 5),
            (1, 9, 4),
            (1, 10, 3),
            (1, 11, 2),

            (2, 1, 10),
            (2, 3, 8),
            (2, 4, 11),
            (2, 5, 7),
            (2, 6, 9),

            (3, 2, 4),
            (3, 7, 3),
            (3, 8, 1),
            (3, 9, 5),
            (3, 11, 6),

            (4, 1, 7),
            (4, 3, 9),
            (4, 5, 11),
            (4, 6, 2),
            (4, 10, 8),

            (5, 2, 5),
            (5, 4, 6),
            (5, 7, 10),
            (5, 9, 1),
            (5, 11, 3),

            (6, 1, 11),
            (6, 3, 2),
            (6, 5, 4),
            (6, 8, 7),
            (6, 10, 9),

            (7, 2, 1),
            (7, 4, 3),
            (7, 6, 5),
            (7, 9, 8),
            (7, 11, 10),

            (8, 1, 4),
            (8, 3, 6),
            (8, 7, 9),
            (8, 8, 11),
            (8, 10, 2),

            (9, 2, 8),
            (9, 4, 10),
            (9, 5, 3),
            (9, 6, 1),
            (9, 11, 7),

            (10, 1, 5),
            (10, 7, 2),
            (10, 8, 4),
            (10, 9, 11),
            (10, 10, 6),

            (11, 2, 9),
            (11, 3, 1),
            (11, 4, 7),
            (11, 5, 10),
            (11, 6, 8)
        };

    /*
       round01		07-06	08-05	09-04	10-03	11-02
       round02		01-10	03-08	04-11	05-07	06-09
       round03		02-04	07-03	08-01	09-05	11-06
       round04		01-07	03-09	05-11	06-02	10-08
       round05		02-05	04-06	07-10	09-01	11-03
       round06		01-11	03-02	05-04	08-07	10-09
       round07		02-01	04-03	06-05	09-08	11-10
       round08		01-04	03-06	07-09	08-11	10-02
       round09		02-08	04-10	05-03	06-01	11-07
       round10		01-05	07-02	08-04	09-11	10-06
       round11		02-09	03-01	04-07	05-10	06-08
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants12 =
        new()
        {
            (1, 7, 6),
            (1, 8, 5),
            (1, 9, 4),
            (1, 10, 3),
            (1, 11, 2),

            (2, 1, 10),
            (2, 3, 8),
            (2, 4, 11),
            (2, 5, 7),
            (2, 6, 9),

            (3, 2, 4),
            (3, 7, 3),
            (3, 8, 1),
            (3, 9, 5),
            (3, 11, 6),

            (4, 1, 7),
            (4, 3, 9),
            (4, 5, 11),
            (4, 6, 2),
            (4, 10, 8),

            (5, 2, 5),
            (5, 4, 6),
            (5, 7, 10),
            (5, 9, 1),
            (5, 11, 3),

            (6, 1, 11),
            (6, 3, 2),
            (6, 5, 4),
            (6, 8, 7),
            (6, 10, 9),

            (7, 2, 1),
            (7, 4, 3),
            (7, 6, 5),
            (7, 9, 8),
            (7, 11, 10),

            (8, 1, 4),
            (8, 3, 6),
            (8, 7, 9),
            (8, 8, 11),
            (8, 10, 2),

            (9, 2, 8),
            (9, 4, 10),
            (9, 5, 3),
            (9, 6, 1),
            (9, 11, 7),

            (10, 1, 5),
            (10, 7, 2),
            (10, 8, 4),
            (10, 9, 11),
            (10, 10, 6),

            (11, 2, 9),
            (11, 3, 1),
            (11, 4, 7),
            (11, 5, 10),
            (11, 6, 8)
        };

    /*
       round01		08-07	09-06	10-05	11-04	12-03	13-02
       round02		01-12	03-10	04-13	05-08	06-11	07-09
       round03		02-04	08-03	09-05	10-01	11-07	13-06
       round04		01-08	03-09	05-11	06-02	07-13	12-10
       round05		02-07	04-06	08-12	09-01	11-03	13-05
       round06		01-11	03-13	05-02	07-04	10-08	12-09
       round07		02-03	04-05	06-07	09-10	11-12	13-01
       round08		01-02	03-04	05-06	08-09	10-11	12-13
       round09		02-12	04-01	06-03	07-05	11-08	13-10
       round10		01-06	03-07	08-13	09-11	10-02	12-04
       round11		02-08	04-10	05-03	06-12	07-01	13-09
       round12		01-05	08-04	09-02	10-06	11-13	12-07
       round13		02-11	03-01	04-09	05-12	06-08	07-10
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants13 =
        new()
        {
            (1, 8, 7),
            (1, 9, 6),
            (1, 10, 5),
            (1, 11, 4),
            (1, 12, 3),
            (1, 13, 2),

            (2, 1, 12),
            (2, 3, 10),
            (2, 4, 13),
            (2, 5, 8),
            (2, 6, 11),
            (2, 7, 9),

            (3, 2, 4),
            (3, 8, 3),
            (3, 9, 5),
            (3, 10, 1),
            (3, 11, 7),
            (3, 13, 6),

            (4, 1, 8),
            (4, 3, 9),
            (4, 5, 11),
            (4, 6, 2),
            (4, 7, 13),
            (4, 12, 10),

            (5, 2, 7),
            (5, 4, 6),
            (5, 8, 12),
            (5, 9, 1),
            (5, 11, 3),
            (5, 13, 5),

            (6, 1, 11),
            (6, 3, 13),
            (6, 5, 2),
            (6, 7, 4),
            (6, 10, 8),
            (6, 12, 9),

            (7, 2, 3),
            (7, 4, 5),
            (7, 6, 7),
            (7, 9, 10),
            (7, 11, 12),
            (7, 13, 1),

            (8, 1, 2),
            (8, 3, 4),
            (8, 5, 6),
            (8, 8, 9),
            (8, 10, 11),
            (8, 12, 13),

            (9, 2, 12),
            (9, 4, 1),
            (9, 6, 3),
            (9, 7, 5),
            (9, 11, 8),
            (9, 13, 10),

            (10, 1, 6),
            (10, 3, 7),
            (10, 8, 13),
            (10, 9, 11),
            (10, 10, 2),
            (10, 12, 4),

            (11, 2, 8),
            (11, 4, 10),
            (11, 5, 3),
            (11, 6, 12),
            (11, 7, 1),
            (11, 13, 9),

            (12, 1, 5),
            (12, 8, 4),
            (12, 9, 2),
            (12, 10, 6),
            (12, 11, 13),
            (12, 12, 7),

            (13, 2, 11),
            (13, 3, 1),
            (13, 4, 9),
            (13, 5, 12),
            (13, 6, 8),
            (13, 7, 10)
        };

    /*
       round01		01-14	04-09	06-07	08-05	10-03	12-02	13-11
       round02		02-10	03-08	05-06	07-04	09-13	11-01	14-12
       round03		01-09	04-05	06-03	08-02	10-12	11-14	13-07
       round04		02-06	03-04	05-13	07-01	09-11	12-08	14-10
       round05		01-05	04-02	06-12	08-10	09-14	11-07	13-03
       round06		02-13	03-01	05-11	07-09	10-06	12-04	14-08
       round07		01-02	04-10	06-08	07-14	09-05	11-03	13-12
       round08		02-11	03-09	05-07	08-04	10-13	12-01	14-06
       round09		01-10	04-06	05-14	07-03	09-02	11-12	13-08
       round10		02-07	03-05	06-13	08-01	10-11	12-09	14-04
       round11		01-06	03-14	05-02	07-12	09-10	11-08	13-04
       round12		02-03	04-01	06-11	08-09	10-07	12-05	13-14
       round13		01-13	03-12	05-10	07-08	09-06	11-04	14-02
    */
    private readonly List<(int Turn, int Home, int Guest)> _numOfParticipants14 =
        new()
        {
            (1, 1, 14),
            (1, 4, 9),
            (1, 6, 7),
            (1, 8, 5),
            (1, 10, 3),
            (1, 12, 2),
            (1, 13, 11),

            (2, 2, 10),
            (2, 3, 8),
            (2, 5, 6),
            (2, 7, 4),
            (2, 9, 13),
            (2, 11, 1),
            (2, 14, 12),

            (3, 1, 9),
            (3, 4, 5),
            (3, 6, 3),
            (3, 8, 2),
            (3, 10, 12),
            (3, 11, 14),
            (3, 13, 7),

            (4, 2, 6),
            (4, 3, 4),
            (4, 5, 13),
            (4, 7, 1),
            (4, 9, 11),
            (4, 12, 8),
            (4, 14, 10),

            (5, 1, 5),
            (5, 4, 2),
            (5, 6, 12),
            (5, 8, 10),
            (5, 9, 14),
            (5, 11, 7),
            (5, 13, 3),

            (6, 2, 13),
            (6, 3, 1),
            (6, 5, 11),
            (6, 7, 9),
            (6, 10, 6),
            (6, 12, 4),
            (6, 14, 8),

            (7, 1, 2),
            (7, 4, 10),
            (7, 6, 8),
            (7, 7, 14),
            (7, 9, 5),
            (7, 11, 3),
            (7, 13, 12),

            (8, 2, 11),
            (8, 3, 9),
            (8, 5, 7),
            (8, 8, 4),
            (8, 10, 13),
            (8, 12, 1),
            (8, 14, 6),

            (9, 1, 10),
            (9, 4, 6),
            (9, 5, 14),
            (9, 7, 3),
            (9, 9, 2),
            (9, 11, 12),
            (9, 13, 8),

            (10, 2, 7),
            (10, 3, 5),
            (10, 6, 13),
            (10, 8, 1),
            (10, 10, 11),
            (10, 12, 9),
            (10, 14, 4),

            (11, 1, 6),
            (11, 3, 14),
            (11, 5, 2),
            (11, 7, 12),
            (11, 9, 10),
            (11, 11, 8),
            (11, 13, 4),

            (12, 2, 3),
            (12, 4, 1),
            (12, 6, 11),
            (12, 8, 9),
            (12, 10, 7),
            (12, 12, 5),
            (12, 13, 14),

            (13, 1, 13),
            (13, 3, 12),
            (13, 5, 10),
            (13, 7, 8),
            (13, 9, 6),
            (13, 11, 4),
            (13, 14, 2)
        };

    /// <summary>
    /// Generates a list of ideal matches using the predefined combinations.
    /// </summary>
    /// <returns>A list of matches represented as <see cref="ValueTuple"/>s of turn and participants.</returns>
    public IList<(int Turn, TP Home, TP Guest)> GenerateMatches()
    {
        return Participants.Count switch {
            5 => ToGenericList(_numOfParticipants5),
            6 => ToGenericList(_numOfParticipants6),
            7 => ToGenericList(_numOfParticipants7),
            8 => ToGenericList(_numOfParticipants8),
            9 => ToGenericList(_numOfParticipants9),
            10 => ToGenericList(_numOfParticipants10),
            11 => ToGenericList(_numOfParticipants11),
            12 => ToGenericList(_numOfParticipants12),
            13 => ToGenericList(_numOfParticipants13),
            14 => ToGenericList(_numOfParticipants14),
            _ => throw new ArgumentOutOfRangeException(nameof(Participants),
                @"The number of participants must be between 5 and 14.")
        };
    }

    private IList<(int Turn, TP Home, TP Guest)> ToGenericList(List<(int Turn, int Home, int Guest)> idealMatches)
    {
        var participants = new List<TP>(Participants);

        var genericMatches = new List<(int Turn, TP Home, TP Guest)>();
        foreach (var idealMatch in idealMatches)
        {
            // Make the result zero-based.
            var (turn, home, guest) = (idealMatch.Turn - 1, participants[idealMatch.Home - 1], participants[idealMatch.Guest - 1]);
            genericMatches.Add((turn, home, guest));
        }

        return genericMatches;
    }
}

