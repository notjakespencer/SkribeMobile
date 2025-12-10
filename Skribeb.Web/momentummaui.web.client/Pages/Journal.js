
import React, { useState, useEffect } from "react";
import { base44 } from "@/api/base44Client";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { motion } from "framer-motion";
import { format } from "date-fns";
import { CheckCircle } from "lucide-react";

import StatsHeader from "../components/journal/StatsHeader";
import Timer from "../components/journal/Timer";
import PromptCard from "../components/journal/PromptCard";
import MoodSelector from "../components/journal/MoodSelector";
import ThemeToggle from "../components/journal/ThemeToggle";

const PROMPTS = [
    "What are three things you're grateful for today?",
    "Describe a moment that made you smile recently.",
    "What's one challenge you're facing, and how can you overcome it?",
    "Who inspires you and why?",
    "What did you learn today?",
    "Describe your perfect day from start to finish.",
    "What's a goal you're working towards?",
    "Write about someone you appreciate and why.",
    "What's something you'd like to improve about yourself?",
    "What makes you unique?",
    "Describe a place where you feel most at peace.",
    "What's a recent accomplishment you're proud of?",
    "What would you tell your younger self?",
    "What are you looking forward to?",
    "Describe a kind act you witnessed or performed.",
    "What's your biggest dream?",
    "What makes you feel energized?",
    "Write about a lesson you learned the hard way.",
    "What's something new you'd like to try?",
    "Describe your ideal future in five years.",
    "What's a problem you solved recently?",
    "Who makes you laugh the most?",
    "What's your favorite way to relax?",
    "What values are most important to you?",
    "Describe a time you stepped out of your comfort zone.",
    "What's something you're curious about?",
    "What hobby brings you joy?",
    "Write about a book, movie, or song that moved you.",
    "What's your favorite memory from childhood?",
    "What makes you feel loved?",
    "Describe your morning routine.",
    "What's something you've been putting off?",
    "What are you most passionate about?",
    "Write about a friendship that matters to you.",
    "What's your greatest strength?",
    "What would you do if you knew you couldn't fail?",
    "Describe a moment of clarity you've had.",
    "What's something that surprised you recently?",
    "What advice would you give to a friend in need?",
    "What makes you feel confident?",
    "Describe your creative process.",
    "What's a tradition you cherish?",
    "What do you need more of in your life?",
    "Write about a person who changed your perspective.",
    "What's your definition of success?",
    "What brings you inner peace?",
    "Describe a fear you've overcome.",
    "What's your favorite season and why?",
    "What motivates you to keep going?",
    "What's something beautiful you noticed today?",
    "Write about your relationship with technology.",
    "What makes your heart feel full?",
    "Describe a random act of kindness you could do.",
    "What's your favorite form of self-care?",
    "What would you like to be remembered for?",
    "Write about a teacher or mentor who influenced you.",
    "What's your relationship with change?",
    "Describe your dream vacation.",
    "What makes you feel grateful for your life?",
    "What's a skill you'd like to master?",
    "Write about your relationship with food.",
    "What's your biggest insecurity and how do you cope?",
    "Describe a moment of pure joy.",
    "What would you do with an extra hour each day?",
    "What's your relationship with money?",
    "Write about a boundary you need to set.",
    "What does home mean to you?",
    "Describe your relationship with your body.",
    "What's something you've always wanted to say?",
    "What makes you feel alive?",
    "Write about your morning or evening rituals.",
    "What's your relationship with social media?",
    "Describe a risk you're glad you took.",
    "What do you need to forgive yourself for?",
    "What's your favorite way to spend a weekend?",
    "Write about your relationship with your parents.",
    "What would you do differently if you could start over?",
    "Describe your workspace or creative space.",
    "What's something you pretend to like but don't?",
    "What makes you feel seen and heard?",
    "Write about your relationship with exercise.",
    "What's a compliment you've received that stuck with you?",
    "Describe your ideal work-life balance.",
    "What's something you wish I knew earlier?",
    "What makes you feel grounded?",
    "Write about a difficult conversation you need to have.",
    "What's your love language?",
    "Describe a moment when you felt truly understood.",
    "What do you do when you're feeling overwhelmed?",
    "What's your relationship with sleep?",
    "Write about a commitment you made to yourself.",
    "What makes you feel most like yourself?",
    "Describe your relationship with music.",
    "What's a belief you've changed your mind about?",
    "What do you need to let go of?",
    "Write about your morning thoughts today.",
    "What's your relationship with nature?",
    "Describe a moment of growth you've experienced.",
    "What makes you feel vulnerable?",
    "What's something you're learning to accept?",
    "Write about your relationship with your siblings.",
    "What would you like to be doing more of?",
    "Describe your ideal morning.",
    "What's something that always makes you feel better?",
    "What's your relationship with failure?",
    "Write about a pattern you've noticed in your life.",
    "What makes you feel creative?",
    "Describe your relationship with time.",
    "What's something you're learning about yourself?",
    "What do you appreciate about your current situation?",
    "Write about a boundary you successfully set.",
    "What's your relationship with your emotions?",
    "Describe a moment when you felt proud of yourself.",
    "What makes you feel connected to others?",
    "What's something you're working on accepting?",
    "Write about your relationship with change and uncertainty.",
    "What would your best friend say about you?",
    "Describe your relationship with your past.",
    "What's something that always brings you comfort?",
    "What makes you feel hopeful?",
    "Write about a recent realization you've had.",
    "What's your relationship with productivity?",
    "Describe a quality you admire in others.",
    "What do you need more patience with?",
    "What's something you're grateful for about yourself?",
    "Write about your relationship with solitude.",
    "What makes you feel safe?",
    "Describe your vision for your future self.",
    "What's a promise you want to make to yourself?",
    "What do you value most in relationships?",
    "Write about something you're learning to embrace.",
    "What's your relationship with your intuition?",
    "Describe a moment of self-discovery.",
    "What makes you feel worthy?",
    "What's something you admire about your life?",
    "Write about your relationship with responsibility.",
    "What do you need to prioritize?",
    "Describe your ideal evening.",
    "What's something you're learning to appreciate?",
    "What makes you feel courageous?",
    "Write about a habit you're trying to build.",
    "What's your relationship with comparison?",
    "Describe a quality you possess that you're proud of.",
    "What do you need to communicate better?",
    "What makes you feel present?",
    "Write about your relationship with your health.",
    "What's something you're learning to celebrate?",
    "Describe a recent moment of gratitude.",
    "What makes you feel supported?",
    "What's your relationship with your goals?",
    "Write about something you're learning to trust.",
    "What do you need to be more gentle with?",
    "Describe your relationship with rest.",
    "What makes you feel balanced?",
    "What's something you're proud of today?",
    "Write about your relationship with growth.",
    "What do you need to honor about yourself?",
    "Describe a recent act of self-love.",
    "What makes you feel empowered?",
    "What's your relationship with your dreams?",
    "Write about something you're learning to honor.",
    "What do you need to acknowledge?",
    "Describe your relationship with your thoughts.",
    "What makes you feel authentic?",
    "What's something you appreciate about this moment?",
    "Write about your relationship with progress.",
    "What do you need to validate?",
    "Describe a recent moment of peace.",
    "What makes you feel whole?",
    "What's your relationship with your worth?",
    "Write about something you're learning to soften towards.",
    "What do you need to celebrate?",
    "Describe your relationship with your journey.",
    "What makes you feel free?",
    "What's something you're grateful for right now?",
    "Write about your relationship with this present moment.",
    "What do you need to embrace?",
    "Describe a quality that defines you.",
    "What makes you feel inspired?",
    "What's your relationship with your future?",
    "Write about something you're learning to love about yourself.",
    "What do you need to nurture?",
    "Describe your relationship with joy.",
    "What makes you feel at home in yourself?",
    "What's something beautiful about your journey?",
    "Write about your relationship with your authentic self.",
    "What do you want to remember about today?",
    "Describe what you're most grateful for in this chapter of life.",
    "What makes you feel like you're exactly where you need to be?",
    "What's your relationship with this version of yourself?",
    "Write about something that fills your heart today.",
    "What do you love most about being alive?",
    "Describe the best part of your day today.",
    "What makes today special?",
    "What's one thing you want to carry forward from today?",
    "Write a love letter to yourself.",
    "What are you most proud of about your journey so far?",
    "Describe what peace means to you right now.",
    "What makes you smile when you think about it?",
    "What's your favorite thing about this season of life?",
    "Write about the person you're becoming.",
    "What do you want to manifest?",
    "Describe your relationship with hope.",
    "What makes your soul happy?",
    "What's something magical that happened recently?",
    "Write about what you're releasing and what you're welcoming.",
    "What do you want to create more of?",
    "Describe what abundance means to you.",
    "What makes you feel radiant?",
    "What's your intention for tomorrow?",
    "Write about what brings you to life.",
    "What are you calling into your reality?",
    "Describe your dream life in vivid detail.",
    "What makes you feel limitless?",
    "What's the best version of yourself like?",
    "Write about what you're manifesting right now.",
    "What do you celebrate about yourself today?",
    "Describe what fulfillment feels like to you.",
    "What makes you believe in magic?",
    "What's your vision for your highest self?",
    "Write about what makes your heart sing.",
    "What are you transforming?",
    "Describe what elevation means to you.",
    "What makes you feel unstoppable?",
    "What's your ultimate vision for your life?",
    "Write about the energy you want to embody.",
    "What are you ascending towards?",
    "Describe your dream reality.",
    "What makes you feel infinite?",
    "What's the most aligned version of you?",
    "Write about what you're becoming.",
    "What do you see when you close your eyes and dream?",
    "Describe your relationship with possibility.",
    "What makes you feel like anything is possible?",
    "What's your vision for your legacy?",
    "Write about the impact you want to make.",
    "What are you most excited about?",
    "Describe what living fully means to you.",
    "What makes you feel like you're thriving?",
    "What's something you want to experience?",
    "Write about your wildest dreams.",
    "What do you want your life to feel like?",
    "Describe your perfect ordinary day.",
    "What makes you feel expansive?",
    "What's your relationship with abundance?",
    "Write about what you're grateful for about your future.",
    "What do you love about this journey?",
    "Describe what makes your life extraordinary.",
    "What makes you feel blessed?",
    "What's something miraculous about your life?",
    "Write about what you appreciate about now.",
    "What do you celebrate about being you?",
    "Describe the gift of today.",
    "What makes this moment precious?",
    "What's beautiful about your story?",
    "Write about what you'll remember about today.",
    "What do you want to savor?",
    "Describe what you're thankful for right now.",
    "What makes life beautiful?",
    "What's your favorite part of your story so far?",
    "Write about what brings you peace today.",
    "What do you cherish most?",
    "Describe what makes you feel rich.",
    "What makes your heart full right now?",
    "What's something wonderful about this day?",
    "Write about what you're holding onto.",
    "What do you want to remember forever?",
    "Describe the perfect ending to today.",
    "What makes right now enough?",
    "What's your favorite thought today?",
    "Write about what you're celebrating.",
    "What do you love about where you are?",
    "Describe what makes today a gift.",
    "What makes you grateful to be here?",
    "What's the most beautiful thing about today?",
    "Write one more thing you're grateful for today.",
    "What final thought do you want to capture from today?",
    "Describe what you'll carry with you from this day.",
    "What makes today worth remembering?",
    "What's the essence of today in one feeling?",
    "Write about the gift this day gave you.",
    "What do you want to whisper thank you for?",
    "Describe how today expanded you.",
    "What makes your heart overflow with gratitude?",
    "What's the magic you found today?",
    "Write about what today taught your soul.",
    "What do you want to hold close from today?",
    "Describe the love you felt today.",
    "What makes today a treasure?",
    "What's the blessing hidden in today?",
    "Write about the light you saw today.",
    "What do you honor about this day?",
    "Describe what made today sacred.",
    "What makes you grateful for the journey?",
    "What's the gift of this present moment?",
    "Write about what your heart wants to say.",
    "What do you want to remember when you close your eyes tonight?",
    "Describe the feeling you want to fall asleep with.",
    "What makes today part of your beautiful story?",
    "What's the last thing you want to acknowledge about today?",
    "Write about why you're grateful for today.",
    "What makes this day a part of who you're becoming?",
    "Describe the final note of gratitude for today.",
    "What do you release into the universe tonight?",
    "What's your closing thought for this day?",
    "Write about what today meant to you.",
    "What do you bless about this day?",
    "Describe the peace you're taking into tomorrow.",
    "What makes you ready for rest?",
    "What's your goodnight to today?",
    "Write your final reflection for day 365.",
];

const calculateLevelFromXp = (xp) => {
    if (xp < 100) return 1;

    // Solve: 50 * L * (L-1) = XP for L
    // Using quadratic formula: L = (1 + sqrt(1 + 8*XP/100)) / 2
    const level = 0.5 * (1 + Math.sqrt(1 + (8 * xp) / 100));
    return Math.floor(level);
};

export default function JournalPage() {
    const queryClient = useQueryClient();
    const [user, setUser] = useState(null);
    const [response, setResponse] = useState("");
    const [isTimerActive, setIsTimerActive] = useState(false);
    const [showMoodSelector, setShowMoodSelector] = useState(false);
    const [hasCompletedToday, setHasCompletedToday] = useState(false);
    const [completionTime, setCompletionTime] = useState(0);
    const [timeLeft, setTimeLeft] = useState(120);

    const today = format(new Date(), "yyyy-MM-dd");
    const dayOfYear = Math.floor((new Date() - new Date(new Date().getFullYear(), 0, 0)) / 86400000);
    const todayPrompt = PROMPTS[dayOfYear % PROMPTS.length];

    const { data: currentUser } = useQuery({
        queryKey: ["currentUser"],
        queryFn: () => base44.auth.me(),
    });

    useEffect(() => {
        if (currentUser) {
            setUser(currentUser);
        }
    }, [currentUser]);

    const { data: todayEntry } = useQuery({
        queryKey: ["todayEntry", today],
        queryFn: async () => {
            const entries = await base44.entities.JournalEntry.filter({ date: today });
            return entries[0] || null;
        },
    });

    useEffect(() => {
        if (todayEntry) {
            setHasCompletedToday(true);
            setResponse(todayEntry.response);
        }
    }, [todayEntry]);

    // Timer Logic moved to parent component
    useEffect(() => {
        if (!isTimerActive) {
            setTimeLeft(120); // Reset timer when not active
            return;
        }

        // This ensures the countdown doesn't wait 1 second for the first tick
        // And also for react's strict mode to prevent double call issues on mount
        let firstTickTimeout;
        if (timeLeft === 120) { // Only set initial timeout if timer just started
            firstTickTimeout = setTimeout(() => {
                setTimeLeft((prev) => (prev > 0 ? prev - 1 : 0));
            }, 50); // A small delay for smoother UI update
        }


        const interval = setInterval(() => {
            setTimeLeft((prev) => {
                if (prev <= 1) {
                    clearInterval(interval);
                    clearTimeout(firstTickTimeout); // Clear this too, if it hasn't fired yet
                    handleTimerComplete();
                    return 0;
                }
                return prev - 1;
            });
        }, 1000);

        return () => {
            clearTimeout(firstTickTimeout);
            clearInterval(interval);
        };
    }, [isTimerActive, timeLeft]); // Add timeLeft to dependencies to react to its changes

    const createEntryMutation = useMutation({
        mutationFn: async ({ mood }) => {
            // Calculate XP with compounding 10% bonus per streak day
            const baseXP = 50;
            const currentStreak = user?.current_streak || 0;

            // Compounding 10% bonus: baseXP * (1.1^streak)
            // This means: Day 1 = 50, Day 2 = 55, Day 3 = 60.5, Day 4 = 66.55, etc.
            const multiplier = Math.pow(1.1, currentStreak);
            const earnedXP = Math.round(baseXP * multiplier);

            const newXP = (user?.total_xp || 0) + earnedXP;
            const newLevel = calculateLevelFromXp(newXP);
            const newStreak = currentStreak + 1;

            await base44.entities.JournalEntry.create({
                date: today,
                prompt: todayPrompt,
                response,
                mood,
                completion_time: completionTime,
            });

            // Update user stats
            await base44.auth.updateMe({
                total_xp: newXP,
                level: newLevel,
                current_streak: newStreak,
                longest_streak: Math.max(newStreak, user?.longest_streak || 0),
            });

            return { newXP, newLevel, newStreak };
        },
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ["todayEntry"] });
            queryClient.invalidateQueries({ queryKey: ["currentUser"] });
            setHasCompletedToday(true);
        },
    });

    const handleTextChange = (e) => {
        const value = e.target.value;
        setResponse(value);

        // Activate timer only if text is entered and it's not already active or completed
        if (value.length > 0 && !isTimerActive && !hasCompletedToday) {
            setIsTimerActive(true);
        }
    };

    const handleTimerComplete = () => {
        setCompletionTime(120); // Full 120 seconds if timer runs out naturally
        setShowMoodSelector(true);
        setIsTimerActive(false); // Stop the timer
    };

    const handleManualComplete = () => {
        const timeTaken = 120 - timeLeft; // Calculate time spent writing
        setCompletionTime(timeTaken);
        setIsTimerActive(false); // Stop the timer
        setShowMoodSelector(true);
    };

    const handleMoodSelect = async (mood) => {
        await createEntryMutation.mutateAsync({ mood });
        setShowMoodSelector(false);
    };

    const toggleTheme = async () => {
        const newTheme = user?.theme === "light" ? "dark" : "light";
        await base44.auth.updateMe({ theme: newTheme });
        // Invalidate currentUser query to refetch user with new theme
        queryClient.invalidateQueries({ queryKey: ["currentUser"] });
    };

    if (!user) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary" />
            </div>
        );
    }

    return (
        <div className="min-h-screen md:ml-20 p-4 md:p-8">
            <div className="max-w-2xl mx-auto">
                {/* Header */}
                <div className="flex items-center justify-between mb-8">
                    <StatsHeader
                        streak={user?.current_streak || 0}
                        level={user?.level || 1}
                        xp={user?.total_xp || 0}
                    />
                    <ThemeToggle theme={user?.theme || "light"} onToggle={toggleTheme} />
                </div>

                {hasCompletedToday ? (
                    <motion.div
                        initial={{ opacity: 0, scale: 0.9 }}
                        animate={{ opacity: 1, scale: 1 }}
                        className="text-center py-16"
                    >
                        <div className="w-24 h-24 mx-auto mb-6 bg-gradient-to-br from-green-400 to-emerald-500 rounded-full flex items-center justify-center">
                            <CheckCircle className="w-12 h-12 text-white" />
                        </div>
                        <h2 className="text-3xl font-bold mb-3 text-foreground">
                            You did it! 🎉
                        </h2>
                        <p className="text-muted-foreground mb-6">
                            You've completed today's journal entry. Come back tomorrow for a new prompt!
                        </p>
                        <div className="bg-card rounded-2xl p-6 border border-border max-w-md mx-auto">
                            <p className="text-sm text-muted-foreground mb-2">Today's entry</p>
                            <p className="text-foreground">{response}</p>
                        </div>
                    </motion.div>
                ) : (
                    <>
                        {/* Timer */}
                        <div className="flex justify-center mb-8">
                            <Timer
                                isActive={isTimerActive}
                                timeLeft={timeLeft} // Pass timeLeft to the Timer component
                            />
                        </div>

                        {/* Prompt */}
                        <div className="mb-6">
                            <PromptCard prompt={todayPrompt} />
                        </div>

                        {/* Response Input */}
                        <motion.div
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: 0.4 }}
                        >
                            <Textarea
                                value={response}
                                onChange={handleTextChange}
                                placeholder="Start writing your thoughts..."
                                className="min-h-[200px] bg-card border-border rounded-2xl p-6 text-foreground placeholder:text-muted-foreground resize-none focus:ring-2 focus:ring-primary"
                                disabled={hasCompletedToday}
                            />
                            <p className="text-xs text-muted-foreground mt-2 text-right">
                                {response.length} characters
                            </p>
                        </motion.div>

                        {/* Manual Complete Button */}
                        {response.length > 0 && isTimerActive && (
                            <motion.div
                                initial={{ opacity: 0, y: 10 }}
                                animate={{ opacity: 1, y: 0 }}
                                className="mt-6"
                            >
                                <Button
                                    onClick={handleManualComplete}
                                    className="w-full bg-primary hover:bg-primary/90 text-primary-foreground py-6 rounded-2xl text-lg font-semibold"
                                >
                                    Complete Entry
                                </Button>
                            </motion.div>
                        )}
                    </>
                )}

                {/* Mood Selector Modal */}
                {showMoodSelector && <MoodSelector onSelect={handleMoodSelect} />}
            </div>
        </div>
    );
}
