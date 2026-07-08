import 'package:flutter/material.dart';

class PrivacySecurityScreen extends StatelessWidget {
  const PrivacySecurityScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: const Text("Privacy & Security"),
        centerTitle: true,
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Header Card
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: colors.primaryContainer,
              borderRadius: BorderRadius.circular(20),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CircleAvatar(
                  radius: 28,
                  backgroundColor: colors.primary,
                  child: Icon(
                    Icons.shield_rounded,
                    color: colors.onPrimary,
                    size: 30,
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        "Learn Securely",
                        style: Theme.of(context).textTheme.titleMedium
                            ?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        "Your personal information, course progress, and AI study sessions are protected using secure technologies. Learnify is committed to providing a safe and private learning experience.",
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 28),

          // Privacy
          _sectionTitle(context, "Privacy"),

          _settingTile(
            context,
            icon: Icons.privacy_tip_outlined,
            title: "Privacy Policy",
            subtitle:
                "Learn how Learnify collects, stores, and protects your information.",
            onTap: () {},
          ),

          _settingTile(
            context,
            icon: Icons.smart_toy_outlined,
            title: "AI Conversations",
            subtitle:
                "Your AI Tutor chats are securely stored to improve your learning experience.",
            onTap: () {},
          ),

          _settingTile(
            context,
            icon: Icons.school_outlined,
            title: "Learning Data",
            subtitle:
                "Manage your saved progress, quizzes, and personalized recommendations.",
            onTap: () {},
          ),

          const SizedBox(height: 24),

          // Security
          _sectionTitle(context, "Security"),

          _settingTile(
            context,
            icon: Icons.password_rounded,
            title: "Change Password",
            subtitle: "Keep your Learnify account secure.",
            onTap: () {},
          ),

          _settingTile(
            context,
            icon: Icons.logout_outlined,
            title: "Sign Out From All Devices",
            subtitle:
                "End active sessions on all devices connected to your account.",
            onTap: () {},
          ),

          _settingTile(
            context,
            icon: Icons.delete_outline_rounded,
            iconColor: Colors.red,
            title: "Clear AI Chat History",
            subtitle: "Delete all saved conversations with your AI Tutor.",
            onTap: () {},
          ),

          const SizedBox(height: 24),

          // Legal
          _sectionTitle(context, "Legal"),

          _settingTile(
            context,
            icon: Icons.description_outlined,
            title: "Terms & Conditions",
            subtitle: "Read Learnify's terms of service.",
            onTap: () {},
          ),

          _settingTile(
            context,
            icon: Icons.help_outline_rounded,
            title: "Help & Support",
            subtitle:
                "Contact us if you have any privacy or security questions.",
            onTap: () {},
          ),

          const SizedBox(height: 40),

          // Footer
          Center(
            child: Column(
              children: [
                Icon(
                  Icons.auto_stories_rounded,
                  size: 34,
                  color: colors.primary,
                ),
                const SizedBox(height: 10),
                Text(
                  "Learnify",
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  "Empowering learning with secure AI.",
                  textAlign: TextAlign.center,
                  style: TextStyle(color: colors.onSurfaceVariant),
                ),
                const SizedBox(height: 10),
                Text(
                  "Version 1.0.0",
                  style: TextStyle(
                    color: colors.onSurfaceVariant,
                    fontSize: 12,
                  ),
                ),
                const SizedBox(height: 20),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _sectionTitle(BuildContext context, String title) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Text(
        title,
        style: Theme.of(
          context,
        ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
      ),
    );
  }

  Widget _settingTile(
    BuildContext context, {
    required IconData icon,
    required String title,
    required String subtitle,
    required VoidCallback onTap,
    Color? iconColor,
  }) {
    final colors = Theme.of(context).colorScheme;

    return Card(
      elevation: 0,
      margin: const EdgeInsets.only(bottom: 10),
      color: colors.surfaceContainerHighest.withOpacity(.35),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        leading: CircleAvatar(
          backgroundColor: (iconColor ?? colors.primary).withOpacity(.12),
          child: Icon(icon, color: iconColor ?? colors.primary),
        ),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Padding(
          padding: const EdgeInsets.only(top: 2),
          child: Text(subtitle),
        ),
        trailing: const Icon(Icons.chevron_right_rounded),
        onTap: onTap,
      ),
    );
  }
}
