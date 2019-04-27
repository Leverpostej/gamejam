﻿using Sirenix.OdinInspector;
//using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance = null;

    [Required]
    public HideGamePanel HidePanel;

    [Required]
    public PlayerTurn ShowPlayerRoleTurn;

    [Required]
    public PlayerTurn PublicClueRevealTurn;

    [Required]
    public PlayerTurn InvestigateLocationTurn;

    [Required]
    public PlayerTurn DiscussionTurn;

    [Required]
    public VotingTurn VotingTurn;

    [Required]
    public MurderWeaponVotingTurn MurderWeaponVotingTurn;

    [Required]
    public EndGame GameoverPanel;

    private Queue<PlayerInfo> turnQueue = new Queue<PlayerInfo>();

    void Awake()
    {
        Instance = this;
        this.StartCoroutine(this.MainGameLoop());
    }

    private IEnumerator MainGameLoop()
    {
        yield return null;

        this.turnQueue = PlayerList.AllPlayers.ToQueue();

        Debug.Log(">Show players their assigned role round");
        while (this.turnQueue.Count > 0)
        {
            var nextPlayer = this.NextPlayer();

            // Show hide panel
            yield return this.StartCoroutine(this.HidePanel.ShowHidePanel(nextPlayer.Name, "FOR YOUR EYES ONLY\nYOU'RE ABOUT TO RECEIVE\nYOUR ROLE IN THIS THRILLER\nIT IS A SECRET FOR YOU ALONE"));

            this.ShowPlayerRoleTurn.gameObject.SetActive(true);
            yield return this.StartCoroutine(this.ShowPlayerRoleTurn.RunTurn(nextPlayer));
            this.ShowPlayerRoleTurn.gameObject.SetActive(false);
        }

        // Public clue reveal.
        Debug.Log(">First clue reveal");
        yield return this.StartCoroutine(this.HidePanel.ShowHidePanel("EVERYONE"));

        this.PublicClueRevealTurn.gameObject.SetActive(true);
        yield return this.StartCoroutine(this.PublicClueRevealTurn.RunTurn(null));
        this.PublicClueRevealTurn.gameObject.SetActive(false);

        // Visit locations
        Debug.Log(">Visit and investigate locations round.");
        this.turnQueue = PlayerList.AllPlayers.Shuffle().ToQueue();
        while (this.turnQueue.Count > 0)
        {
            var nextPlayer = this.NextPlayer();
            yield return this.StartCoroutine(this.HidePanel.ShowHidePanel(nextPlayer.Name, "THE TRUTH IS FOR YOUR EYES ONLY\nYOU MAY TELL THEM LIES\nYOU MAY TELL THEM TRUTH\nTHE CHOICE IS YOURS"));

            this.InvestigateLocationTurn.gameObject.SetActive(true);
            yield return this.StartCoroutine(this.InvestigateLocationTurn.RunTurn(nextPlayer));
            this.InvestigateLocationTurn.gameObject.SetActive(false);

            //yield return this.HidePanel.ShowHidePanel("EVERYONE");
            this.DiscussionTurn.gameObject.SetActive(true);
            yield return this.StartCoroutine(this.DiscussionTurn.RunTurn(null));
            this.DiscussionTurn.gameObject.SetActive(false);
        }

        // Voting turn!
        Debug.Log(">Voting turn.");
        this.turnQueue = PlayerList.AllPlayers
            .Where(x => x.Role != PlayerRole.Ghost)
            .Shuffle()
            .Append(PlayerList.Ghost)
            .ToQueue();
        while (this.turnQueue.Count > 0)
        {
            var nextPlayer = this.NextPlayer();
            yield return this.StartCoroutine(this.HidePanel.ShowHidePanel(nextPlayer.Name, "YOU ARE ABOUT TO VOTE\nON WHO YOU THINK DID IT\nTHE GHOST MUST TAKE IT\nINTO CONSIDERATION\nPICK CAREFULLY"));

            this.VotingTurn.gameObject.SetActive(true);
            yield return this.StartCoroutine(this.VotingTurn.RunTurn(nextPlayer));
            this.VotingTurn.gameObject.SetActive(false);
        }

        // Ghost voting for murder weapon!
        Debug.Log(">Ghost vote for murder weapon.");
        this.MurderWeaponVotingTurn.gameObject.SetActive(true);
        yield return this.StartCoroutine(this.MurderWeaponVotingTurn.RunTurn(PlayerList.Ghost));
        this.MurderWeaponVotingTurn.gameObject.SetActive(false);

        Debug.Log(">End of game.");
        yield return this.StartCoroutine(this.HidePanel.ShowHidePanel("EVERYONE", "THE TRUTH IS ABOUT TO BE REVEALED..."));
        this.GameoverPanel.gameObject.SetActive(true);
        this.GameoverPanel.BuildEndGame(this.VotingTurn.GhostVotedPlayer, this.MurderWeaponVotingTurn.VotedWeapon);
    }

    public PlayerInfo NextPlayer()
    {
        var nextPlayer = this.turnQueue.Dequeue();
        Debug.Log("Next player: " + nextPlayer.Name);
        return nextPlayer;
    }
}


public static class CollectionExtenions
{
    public static Queue<T> ToQueue<T>(this IEnumerable<T> collection)
    {
        var queue = new Queue<T>();
        foreach (var item in collection)
        {
            queue.Enqueue(item);
        }

        return queue;
    }

    public static T PickRandom<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();

        for (int i = list.Count - 1; i > 0; i--)
        {
            var r = UnityEngine.Random.Range(0, i);
            var a = list[i];
            list[i] = list[r];
            list[r] = a;
        }

        for (int i = 0; i < list.Count; i++)
        {
            yield return list[i];
        }
    }
}