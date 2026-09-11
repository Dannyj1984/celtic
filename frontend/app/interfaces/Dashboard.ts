export interface IDashboardData {
    parentName: string;
    playerName: string;
    subscriptionStatus: string;
    nextSubPaymentDate: string;
    coachWhatsAppNumber: string;
    cardsProgress: {
        cardsCount: number;
        cardsUntilNextReward: number;
        nextReward: {
            cardsRequired: number;
            rewardText: string;
        };
        unlockedRewards: string[];
    };
    attendingNextTraining: boolean;
    attendingNextMatch: boolean;
    coachNotes?: string;
    allowPhotos?: boolean;
    nextMatch: {
        id: string;
        date: string;
        opposition: string;
        location: string;
        status: string;
        teamId?: string | null;
        teamName?: string | null;
    };
    trainingSchedule: {
        day: string;
        startTime: string;
        endTime: string;
        location: string;
        trainingFocus?: string;
        goodToKnow?: string;
    };
    performance: {
        training: {
            totalSessions: number;
            attendedSessions: number;
            percentage: number;
        };
        matches: {
            totalSessions: number;
            attendedSessions: number;
            percentage: number;
        };
    };
}