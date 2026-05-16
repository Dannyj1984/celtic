export interface IDashboardData {
    parentName: string;
    playerName: string;
    subscriptionStatus: string;
    nextSubPaymentDate: string;
    coachWhatsAppNumber: string;
    attendingNextTraining: boolean;
    attendingNextMatch: boolean;
    coachNotes?: string;
    nextMatch: {
        id: string;
        date: string;
        opposition: string;
        location: string;
        status: string;
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