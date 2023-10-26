using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaifugoDecideInfo
{
    public int[] infoVelues = new int[4]; // 決定数
}


public class DaifugoDefine
{
    // 役のタイプ
    public enum eCardSetType
    {
        None,

        Single, // 一枚
        Multi, // 複数枚
        Step, // 階段

        Illegal, // 不正

        Max
    }
    // カードの識別子から番号に変換
    public static int CardIdentifierToNumber(int cardIdentifier)
    {
        return cardIdentifier / 10;
    }

    // カードの識別子からマーク（スート）に変換
    public static int CardIdentifierToSuit(int cardIdentifier)
    {
        return cardIdentifier % 10;
    }

    // カードの識別子から強さを求める
    public static int CardIdentifierToStrength(int cardIdentifier)
    {
        int number = cardIdentifier / 10;
        if(number < 3)
        {
            return number - 3 + 13;
        }
        else
        {
            return number - 3;
        }
    }

    // カード同士の強さを比較
    public static int  CompareCardIdentifier(int a, int b)
    {
        int strengthA = CardIdentifierToStrength(a);
        int strengthB = CardIdentifierToStrength(b);

        return strengthA - strengthB;
    }


    /// <summary>
    /// 役判定
    /// </summary>
    /// <param name="cardList"></param>
    /// <param name="cardStrength"></param>
    /// <returns></returns>
    public static eCardSetType CheckCardSetType(List<int> cardList, out int cardStrength)
    {
        cardStrength = 0;

        // 未選択
        if(cardList.Count== 0) {
            return eCardSetType.None;

        }

        if(cardList.Count == 1) // 一枚なら
        {
            cardStrength = CardIdentifierToStrength(cardList[0]); // 強さゲット
            return eCardSetType.Single; 
        }

        // 複数枚
        if(CheckCardMulti(cardList))
        {
            cardStrength = CardIdentifierToStrength(cardList[cardList.Count-1]);
            return eCardSetType.Multi;
        }

        
        // 階段
        if (CheckCardStep(cardList))
        {
            cardStrength = CardIdentifierToStrength(cardList[cardList.Count - 1]);
            return eCardSetType.Step;
        }

        // 無効な役
        return eCardSetType.Illegal;

    }

    /// <summary>
    /// 複数枚かどうか判定
    /// </summary>
    /// <param name="cardList"></param>
    /// <returns></returns>
    public static bool CheckCardMulti(List<int > cardList)
    {
        int n = cardList.Count;

        // 2枚未満は複数ではない
        if(n < 2)
        {
            return false;
        }

        // 2枚以上でも強さが異なっている => 複数ではない
        for(int i = 1; i < n; i++)
        {
            if (CardIdentifierToStrength(cardList[i]) != CardIdentifierToStrength(cardList[i -1]))
            {
                return false;
            }
        }
        return true;
    }


    public static bool CheckCardStep(List<int> cardList)
    {
        int n = cardList.Count;

        // 3枚未満階段ではない
        if (n < 3)
        {
            return false;
        }

        // スートが異なっている場合は階段ではない
        for (int i = 1; i < n; i++)
        {
            if (CardIdentifierToSuit(cardList[i]) != CardIdentifierToSuit(cardList[i - 1]))
            {
                return false;
            }
        }

        // 強さが連続していなければ階段ではない
        for (int i = 1; i < n; i++)
        {
            if (CardIdentifierToStrength(cardList[i]) != CardIdentifierToStrength(cardList[i - 1] + 1))
            {
                return false;
            }
        }
        return true;
    
    }
}
