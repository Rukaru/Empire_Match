using System;
using System.Collections.Generic;
using UnityEngine;

public static class RankingManager
{
    const int MaxRank = 10;
    const string Key = "RANKING_V1"; // バージョン変えたい時はここ変える

    [Serializable]
    public class Entry
    {
        public int score;
        public int turns;
        public string date; // 表示用（任意）

        public Entry(int score, int turns)
        {
            this.score = score;
            this.turns = turns;
            date = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        }
    }

    [Serializable]
    class EntryList
    {
        public List<Entry> list = new List<Entry>();
    }

    public static List<Entry> Load()
    {
        if (!PlayerPrefs.HasKey(Key)) return new List<Entry>();

        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json)) return new List<Entry>();

        try
        {
            var wrapper = JsonUtility.FromJson<EntryList>(json);
            return wrapper?.list ?? new List<Entry>();
        }
        catch
        {
            return new List<Entry>();
        }
    }

    public static void Save(List<Entry> list)
    {
        var wrapper = new EntryList { list = list };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    public static void AddScore(int score, int turns)
    {
        var list = Load();
        list.Add(new Entry(score, turns));

        // スコア高い順、同点ならターン少ない順
        list.Sort((a, b) =>
        {
            int c = b.score.CompareTo(a.score);
            if (c != 0) return c;
            return a.turns.CompareTo(b.turns);
        });

        if (list.Count > MaxRank) list.RemoveRange(MaxRank, list.Count - MaxRank);
        Save(list);
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}