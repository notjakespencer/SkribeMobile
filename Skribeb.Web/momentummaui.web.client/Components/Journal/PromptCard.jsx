import React from "react";
import { motion } from "framer-motion";
import { Sparkles } from "lucide-react";

export default function PromptCard({ prompt }) {
    return (
        <motion.div

            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}

            className="bg-gradient-to-br from-primary/10 to-purple-500/10 rounded-3xl p-6 border border-primary/20 shadow-lg text-center"
        >
            <div className="flex flex-col items-center justify-center">
                <div className="p-3 bg-primary/20 rounded-2xl mb-4">
                    <Sparkles className="w-6 h-6 text-primary" />
                </div>
                <h3 className="text-xl font-semibold text-foreground leading-relaxed max-w-md mx-auto">
                    {prompt}
                </h3>
            </div>
        </motion.div>
    );
}