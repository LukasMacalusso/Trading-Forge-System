import { LandingNavbar } from './LandingNavbar';
import { HeroSection } from './HeroSection';
import { HowItWorksSection } from './HowItWorksSection';
import { PlatformPreviewSection } from './PlatformPreviewSection';
import { BenefitsSection } from './BenefitsSection';
import { PricingSection } from './PricingSection';
import { FinalCTASection } from './FinalCTASection';
import { LandingFooter } from './LandingFooter';

export function LandingPage() {
  return (
    <div className="min-h-screen bg-neutral-950 overflow-x-hidden">
      <LandingNavbar />
      <HeroSection />
      <HowItWorksSection />
      <PlatformPreviewSection />
      <BenefitsSection />
      <PricingSection />
      <FinalCTASection />
      <LandingFooter />
    </div>
  );
}
